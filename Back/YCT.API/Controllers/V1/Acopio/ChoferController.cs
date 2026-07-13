using System.Globalization;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Application.UseCases.Acopio.Chofer.Login;
using YCT.Application.UseCases.Acopio.Planillas.Save;
using YCT.Domain.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.API.Controllers.V1.Acopio;

/// <summary>
/// Endpoints expuestos a la app móvil del chofer (YCT Chofer).
/// Requiere login de conductor (cédula + PIN) → JWT rol Conductor.
/// </summary>
[ApiController]
[Route("api/acopio/chofer")]
[Authorize(Roles = Roles.Conductor)]
public class ChoferController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IGenericRepository<Ruta> _rutaRepo;
    private readonly IGenericRepository<Camion> _camionRepo;
    private readonly IGenericRepository<Granjero> _granjeroRepo;
    private readonly IGenericRepository<GranjeroCodigo> _codigoRepo;
    private readonly IGenericRepository<RutaNovedad> _novedadRepo;
    private readonly IGenericRepository<Conductor> _conductorRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWhatsAppNotifier _whatsapp;
    private readonly ILogger<ChoferController> _logger;

    public ChoferController(
        IMediator mediator,
        IGenericRepository<Ruta> rutaRepo,
        IGenericRepository<Camion> camionRepo,
        IGenericRepository<Granjero> granjeroRepo,
        IGenericRepository<GranjeroCodigo> codigoRepo,
        IGenericRepository<RutaNovedad> novedadRepo,
        IGenericRepository<Conductor> conductorRepo,
        IUnitOfWork unitOfWork,
        IWhatsAppNotifier whatsapp,
        ILogger<ChoferController> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _rutaRepo = rutaRepo;
        _camionRepo = camionRepo;
        _granjeroRepo = granjeroRepo;
        _codigoRepo = codigoRepo;
        _novedadRepo = novedadRepo;
        _conductorRepo = conductorRepo;
        _whatsapp = whatsapp;
        _logger = logger;
    }

    /// <summary>Login del conductor: cédula + PIN → token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] ChoferLoginCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Lista de granjeros activos con sus códigos/fincas (para la captura en la app del chofer).</summary>
    [HttpGet("granjeros")]
    public async Task<IActionResult> Granjeros()
    {
        var codigos = (await _codigoRepo.FindAsync(c => c.IsActive))
            .GroupBy(c => c.GranjeroId)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Codigo).ToList());

        var granjeros = (await _granjeroRepo.FindAsync(g => g.IsActive))
            .OrderBy(g => g.Numero)
            .Select(g => new
            {
                id = g.Id,
                numero = g.Numero,
                nombreCompleto = g.NombreCompleto,
                finca = g.Finca,
                codigos = (codigos.TryGetValue(g.Id, out var cs) ? cs : new List<GranjeroCodigo>())
                    .Select(c => new { id = c.Id, codigo = c.Codigo, finca = c.Finca, tinasYct = c.TinasYct })
                    .ToList()
            })
            .ToList();
        return Ok(ResponseBase<object>.Ok(granjeros));
    }

    /// <summary>Recibe la planilla enviada por el chofer desde la app móvil.</summary>
    [HttpPost("recogidas")]
    public async Task<IActionResult> EnviarRecogidas([FromBody] ChoferEnvioRequest req)
    {
        if (req.Recogidas == null || req.Recogidas.Count == 0)
            return BadRequest(ResponseBase<object>.Fail("No hay recogidas en el envío"));

        // El conductor se identifica por el token (no se confía en el body).
        var conductorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Generar código (CAMION-FECHA-HHMM) si no existe ya una ruta del día/camión/conductor
        var camion = await _camionRepo.GetByIdAsync(req.CamionId);
        if (camion == null) return BadRequest(ResponseBase<object>.Fail("Camión no encontrado"));

        var fecha = req.Fecha.Date;

        // ===== Determinar sobre qué ruta guardar =====
        int? rutaId = null;
        string codigo;

        // 1) Idempotencia por UUID del chofer: si este mismo envío ya se recibió antes
        //    (reintento por conexión lenta / respuesta perdida), se reconoce la MISMA
        //    ruta en vez de crear un duplicado (-2, -3). Así la tablet recibe éxito y
        //    deja de mostrar "pendiente".
        var porUuid = string.IsNullOrWhiteSpace(req.Uuid) ? null
            : (await _rutaRepo.FindAsync(r => r.ChoferUuid == req.Uuid)).FirstOrDefault();
        if (porUuid != null)
        {
            // Si ya fue procesada en planta (finalizada), no se re-guarda para no pisar
            // lo que hizo el receptor: solo se confirma como recibida.
            if (porUuid.Status is "Conciliada" or "Anulada" or "PendienteAutorizacion")
                return Ok(ResponseBase<object>.Ok(new { porUuid.Id, porUuid.Codigo }, "Planilla ya recibida"));
            rutaId = porUuid.Id;
            codigo = porUuid.Codigo;
        }
        else
        {
            // 2) UUID nuevo = planilla distinta → SIEMPRE una ruta NUEVA (con sufijo si
            //    ya hay rutas del día). NUNCA se sobrescribe otra ruta del día: hacerlo
            //    borraba sus recogidas (ej. una segunda ruta del chofer pisaba la primera
            //    de 8 granjas y solo quedaban las 2 nuevas). Los reenvíos del mismo envío
            //    ya se manejan arriba por UUID, así que aquí nunca hay que "actualizar".
            var rutasDelDia = (await _rutaRepo.FindAsync(r =>
                r.Fecha == fecha && r.CamionId == req.CamionId && r.ConductorId == conductorId)).ToList();
            var baseCodigo = $"{camion.Nombre}-{fecha:ddMM}";
            codigo = rutasDelDia.Count == 0 ? baseCodigo : $"{baseCodigo}-{rutasDelDia.Count + 1}";
            // rutaId queda null → se crea una ruta nueva, preservando las anteriores.
        }

        var saveCmd = new SavePlanillaCommand(
            rutaId,
            codigo,
            fecha,
            req.CamionId,
            conductorId,
            req.AsistenteId,
            req.HoraSalida,
            null, // HoraLlegada → la registra el operario planta
            null, // HoraDescargue → idem
            $"[Enviado por chofer · UUID {req.Uuid}]",
            req.Recogidas.Select((r, i) => new SavePlanillaItemRequest
            {
                Id = null,
                GranjeroId = r.GranjeroId,
                GranjeroCodigoId = r.GranjeroCodigoId,
                Fecha = fecha,
                Cantinas = r.Cantinas,
                SaldoLitros = r.SaldoLitros,
                LitrosRegaladosChofer = r.LitrosRegaladosChofer,
                Observacion = r.Observacion,
                EstadoVista = r.EstadoVista,
                EstadoOlor = r.EstadoOlor,
                EstadoSabor = r.EstadoSabor,
                Orden = i + 1,
                CapturadoAt = DateTime.TryParse(r.CapturadoAt, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var cap) ? cap : null,
                GpsLat = r.GpsLat,
                GpsLng = r.GpsLng
            }).ToList(),
            req.Uuid
        );

        var result = await _mediator.Send(saveCmd);
        if (!result.Success) return BadRequest(result);

        // La ruta ya existe: ligar las novedades que el chofer reportó DURANTE el recorrido
        // (llegaron antes que la planilla, por eso venían sueltas atadas solo por UUID).
        await LigarNovedadesAsync(req.Uuid);

        return Ok(result);
    }

    /// <summary>
    /// Novedad reportada por el chofer en plena ruta. Se registra apenas ocurre (la ruta
    /// aún no existe en el servidor) y avisa por WhatsApp. Idempotente por UUID: la tablet
    /// reenvía hasta confirmar, y un reenvío no puede crear duplicados.
    /// </summary>
    [HttpPost("novedades")]
    public async Task<IActionResult> ReportarNovedad([FromBody] ChoferNovedadRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Uuid))
            return BadRequest(ResponseBase<object>.Fail("Falta el identificador de la novedad"));
        if (string.IsNullOrWhiteSpace(req.Tipo))
            return BadRequest(ResponseBase<object>.Fail("Falta el tipo de novedad"));
        // "Otro" sin explicación no sirve para nada a quien la lee en la sede.
        if (req.Tipo.Equals("Otro", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(req.Descripcion))
            return BadRequest(ResponseBase<object>.Fail("La novedad 'Otro' necesita una descripción"));

        var conductorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Idempotencia: si ya la teníamos, se confirma sin duplicar ni volver a avisar.
        var existente = (await _novedadRepo.FindAsync(n => n.Uuid == req.Uuid)).FirstOrDefault();
        if (existente != null)
            return Ok(ResponseBase<object>.Ok(new { existente.Id }, "Novedad ya recibida"));

        var reportadoAt = DateTime.TryParse(req.ReportadoAt, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var r) ? r : ColombiaTime.Now;

        // Si la ruta ya existe (el chofer envió la planilla antes), se liga de una.
        var ruta = string.IsNullOrWhiteSpace(req.PlanillaUuid) ? null
            : (await _rutaRepo.FindAsync(x => x.ChoferUuid == req.PlanillaUuid)).FirstOrDefault();

        var novedad = new RutaNovedad
        {
            Uuid = req.Uuid,
            PlanillaUuid = req.PlanillaUuid ?? string.Empty,
            RutaId = ruta?.Id,
            ConductorId = conductorId,
            CamionId = req.CamionId,
            Categoria = req.Categoria,
            Tipo = req.Tipo,
            Descripcion = req.Descripcion,
            GranjeroCodigoId = req.GranjeroCodigoId,
            ReportadoAt = reportadoAt,
            GpsLat = req.GpsLat,
            GpsLng = req.GpsLng,
        };
        await _novedadRepo.AddAsync(novedad);
        // AddAsync solo la mete al change tracker: sin esto NO se guarda nada (la tabla
        // quedaba vacía, se devolvía 200 con Id=0 y, como la idempotencia busca una fila
        // que no existía, cada reintento mandaba OTRO WhatsApp).
        await _unitOfWork.SaveChangesAsync();

        // Se avisa DESPUÉS de guardar: nunca alertar de algo que no quedó registrado.
        await AvisarNovedadAsync(novedad);

        return Ok(ResponseBase<object>.Ok(new { novedad.Id }, "Novedad registrada"));
    }

    /// <summary>Ata a la ruta recién creada las novedades que llegaron antes que la planilla.</summary>
    private async Task LigarNovedadesAsync(string? planillaUuid)
    {
        if (string.IsNullOrWhiteSpace(planillaUuid)) return;
        var ruta = (await _rutaRepo.FindAsync(r => r.ChoferUuid == planillaUuid)).FirstOrDefault();
        if (ruta == null) return;

        var sueltas = (await _novedadRepo.FindAsync(n => n.PlanillaUuid == planillaUuid && n.RutaId == null)).ToList();
        if (sueltas.Count == 0) return;

        foreach (var n in sueltas)
        {
            n.RutaId = ruta.Id;
            await _novedadRepo.UpdateAsync(n);
        }
        await _unitOfWork.SaveChangesAsync(); // sin esto el vínculo no se persistía
    }

    /// <summary>Aviso por WhatsApp. Best-effort: si falla, la novedad YA quedó registrada.</summary>
    private async Task AvisarNovedadAsync(RutaNovedad n)
    {
        try
        {
            var conductor = await _conductorRepo.GetByIdAsync(n.ConductorId);
            var camion = await _camionRepo.GetByIdAsync(n.CamionId);
            var finca = n.GranjeroCodigoId.HasValue
                ? (await _codigoRepo.GetByIdAsync(n.GranjeroCodigoId.Value))?.Finca
                : null;

            await _whatsapp.SendNovedadAsync(new WhatsAppNovedadModel(
                n.Tipo,
                n.Categoria,
                conductor?.NombreCompleto ?? $"#{n.ConductorId}",
                camion?.Nombre ?? $"#{n.CamionId}",
                n.ReportadoAt,
                n.Descripcion ?? "-",
                finca ?? "-"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo avisar la novedad {Uuid} por WhatsApp (queda registrada igual)", n.Uuid);
        }
    }
}
