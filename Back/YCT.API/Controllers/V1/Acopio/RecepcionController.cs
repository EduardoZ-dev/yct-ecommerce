using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Application.UseCases.Acopio.ClientesTerceros.ConfirmarEntrega;
using YCT.Application.UseCases.Acopio.Planillas.ValidatePlanta;
using YCT.Application.UseCases.Acopio.Recepcion.Login;
using YCT.Domain.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.API.Controllers.V1.Acopio;

/// <summary>
/// Endpoints de la tablet de recepción en planta (YCT Recepción).
/// VALIDACIÓN A CIEGAS: el receptor NUNCA ve lo que declaró el chofer ni la diferencia;
/// solo recibe la lista de camiones por descargar e ingresa los litros que midió.
/// Requiere login de recepción → JWT rol Recepcion.
/// </summary>
[ApiController]
[Route("api/acopio/recepcion")]
[Authorize(Roles = Roles.Recepcion)]
public class RecepcionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IGenericRepository<Ruta> _rutaRepo;
    private readonly IGenericRepository<Camion> _camionRepo;
    private readonly IGenericRepository<Conductor> _conductorRepo;
    private readonly IGenericRepository<Recogida> _recogidaRepo;
    private readonly IGenericRepository<Granjero> _granjeroRepo;
    private readonly IGenericRepository<GranjeroCodigo> _codigoRepo;
    private readonly IGenericRepository<EntregaTercero> _entregaTerceroRepo;

    public RecepcionController(
        IMediator mediator,
        IGenericRepository<Ruta> rutaRepo,
        IGenericRepository<Camion> camionRepo,
        IGenericRepository<Conductor> conductorRepo,
        IGenericRepository<Recogida> recogidaRepo,
        IGenericRepository<Granjero> granjeroRepo,
        IGenericRepository<GranjeroCodigo> codigoRepo,
        IGenericRepository<EntregaTercero> entregaTerceroRepo)
    {
        _mediator = mediator;
        _rutaRepo = rutaRepo;
        _camionRepo = camionRepo;
        _conductorRepo = conductorRepo;
        _recogidaRepo = recogidaRepo;
        _granjeroRepo = granjeroRepo;
        _codigoRepo = codigoRepo;
        _entregaTerceroRepo = entregaTerceroRepo;
    }

    /// <summary>Login de la tablet de recepción: usuario + clave → token JWT (rol Recepcion).</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] RecepcionLoginCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>
    /// Planillas de HOY pendientes de descargar (Status = EsperandoDescargue).
    /// A CIEGAS: sin litros del chofer ni diferencia.
    /// </summary>
    [HttpGet("pendientes")]
    public async Task<IActionResult> Pendientes()
    {
        var hoy = ColombiaTime.Today;
        var rutas = (await _rutaRepo.FindAsync(r =>
                r.Status == "EsperandoDescargue" && r.Fecha >= hoy && r.Fecha < hoy.AddDays(1)))
            .OrderBy(r => r.UpdatedAt ?? r.CreatedAt)
            .ToList();

        if (rutas.Count == 0)
            return Ok(ResponseBase<List<RecepcionPendienteDto>>.Ok(new List<RecepcionPendienteDto>()));

        var camiones = (await _camionRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.Nombre);
        var conductores = (await _conductorRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.NombreCompleto);
        var granjeros = (await _granjeroRepo.GetAllAsync()).ToDictionary(g => g.Id, g => g.NombreCompleto);
        var codigos = (await _codigoRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c);
        var rutaIds = rutas.Select(r => r.Id).ToHashSet();
        var recogidasPorRuta = (await _recogidaRepo.FindAsync(r => rutaIds.Contains(r.RutaId)))
            .GroupBy(r => r.RutaId)
            .ToDictionary(g => g.Key, g => g.ToList());

        static string? EstadosNoNormales(Recogida r)
        {
            var malos = new[] { r.EstadoVista, r.EstadoOlor, r.EstadoSabor }
                .Where(e => !string.IsNullOrWhiteSpace(e) && e != "Normal")
                .ToList();
            return malos.Count > 0 ? string.Join(" · ", malos) : null;
        }

        var dtos = rutas.Select(r =>
        {
            var recs = recogidasPorRuta.TryGetValue(r.Id, out var rr) ? rr : new List<Recogida>();
            var novedades = recs
                .Select(rec =>
                {
                    var estado = EstadosNoNormales(rec);
                    var obs = string.IsNullOrWhiteSpace(rec.Observacion) ? null : rec.Observacion!.Trim();
                    if (estado == null && obs == null) return (RecepcionNovedadDto?)null; // sin novedad
                    var codigo = rec.GranjeroCodigoId.HasValue && codigos.TryGetValue(rec.GranjeroCodigoId.Value, out var c) ? c : null;
                    return new RecepcionNovedadDto
                    {
                        Codigo = codigo?.Codigo ?? string.Empty,
                        Granjero = granjeros.TryGetValue(rec.GranjeroId, out var gn) ? gn : $"#{rec.GranjeroId}",
                        Finca = codigo?.Finca,
                        Estado = estado,
                        Observacion = obs
                    };
                })
                .Where(n => n != null)
                .Select(n => n!)
                .ToList();

            return new RecepcionPendienteDto
            {
                Id = r.Id,
                Codigo = r.Codigo,
                Fecha = r.Fecha,
                CamionNombre = camiones.TryGetValue(r.CamionId, out var cn) ? cn : $"#{r.CamionId}",
                ConductorNombre = conductores.TryGetValue(r.ConductorId, out var kn) ? kn : $"#{r.ConductorId}",
                NumFincas = recs.Count,
                EnviadoAt = ColombiaTime.FromUtc(r.UpdatedAt ?? r.CreatedAt),
                NovedadesLeche = novedades
            };
        }).ToList();

        return Ok(ResponseBase<List<RecepcionPendienteDto>>.Ok(dtos));
    }

    /// <summary>
    /// Registra el descargue: los litros que midió el receptor + observación.
    /// Internamente concilia contra lo del chofer, pero DEVUELVE SOLO ok (sin diferencia),
    /// para que el receptor no pueda inferir lo esperado.
    /// </summary>
    [HttpPost("validar")]
    public async Task<IActionResult> Validar([FromBody] RecepcionValidarRequest req)
    {
        var result = await _mediator.Send(new ValidatePlantaCommand
        {
            Id = req.PlanillaId,
            TotalLitrosPlanta = req.LitrosPlanta,
            CantinasPlanta = req.CantinasPlanta,
            LitrosSueltosPlanta = req.LitrosSueltosPlanta,
            HoraDescargue = ColombiaTime.Now.TimeOfDay,
            Observaciones = req.Observacion
        });

        // Respuesta neutra: nunca exponemos la diferencia ni lo del chofer a esta tablet.
        if (!result.Success)
            return BadRequest(ResponseBase<object>.Fail(result.Message));
        return Ok(ResponseBase<object>.Ok(new { ok = true }, "Descargue registrado"));
    }

    // ===== Clientes terceros =====
    // El panel REGISTRA la entrega; la tablet solo REAFIRMA que esa leche llegó.

    /// <summary>
    /// Cuántos días hacia atrás una entrega SIN confirmar sigue apareciendo en la tablet. El panel
    /// puede fechar la entrega con el día de una planilla anterior (al validar un descargue atrasado);
    /// si la tablet solo mirara "hoy", esa entrega nunca se vería ni se podría confirmar.
    /// </summary>
    private const int DiasPorConfirmar = 7;

    /// <summary>
    /// Entregas de clientes terceros para reafirmar su llegada: todas las de HOY y las de los
    /// últimos <see cref="DiasPorConfirmar"/> días que siguen sin confirmar.
    /// A CIEGAS: sin precio ni valor, solo quién trajo la leche y cuánta.
    /// </summary>
    [HttpGet("terceros")]
    public async Task<IActionResult> Terceros()
    {
        var hoy = ColombiaTime.Today;
        var manana = hoy.AddDays(1);
        var desde = hoy.AddDays(-DiasPorConfirmar);
        // Se incluye el cliente en la misma consulta para no pedir su nombre entrega por entrega.
        var entregas = await _entregaTerceroRepo.FindAsync(
            e => e.Fecha < manana
                 && (e.Fecha >= hoy || (e.Fecha >= desde && e.ConfirmadaEnPlantaAt == null)),
            e => e.ClienteTercero);

        var dtos = entregas
            .OrderBy(e => e.Fecha)
            .ThenBy(e => e.CreatedAt)
            .Select(e => new RecepcionTerceroDto
            {
                Id = e.Id,
                Fecha = e.Fecha,
                ClienteNombre = e.ClienteTercero.NombreCompleto,
                Municipio = e.ClienteTercero.Municipio,
                Confirmada = e.ConfirmadaEnPlantaAt.HasValue,
                ConfirmadaEnPlantaAt = e.ConfirmadaEnPlantaAt,
                CantinasPlanta = e.CantinasPlanta,
                SaldoPlanta = e.SaldoPlanta,
                LitrosPlanta = e.LitrosPlanta,
                RegistradoPorNombre = e.RegistradoPorNombre,
                CreatedAt = e.CreatedAt
            })
            .ToList();

        return Ok(ResponseBase<List<RecepcionTerceroDto>>.Ok(dtos));
    }

    /// <summary>
    /// Reafirma que la leche de esa entrega llegó. Idempotente (el command no reescribe una
    /// confirmación previa) y limitado a la misma ventana que muestra la tablet: lo más viejo se
    /// confirma desde el panel.
    /// </summary>
    [HttpPost("terceros/{id}/confirmar")]
    public async Task<IActionResult> ConfirmarTercero(int id, [FromBody] RecepcionConfirmarTerceroRequest? body)
    {
        // En planta la llegada se confirma MIDIENDO: sin cantidades no se acepta (una app vieja
        // que no las manda recibe el aviso de actualizarse en vez de confirmar a ciegas de verdad).
        if (body?.Cantinas is null || body.SaldoLitros is null)
            return BadRequest(ResponseBase<bool>.Fail(
                "Indica cuánta leche llegó (cantinas y saldo). Si la app no lo pide, actualízala."));

        var entrega = await _entregaTerceroRepo.GetByIdAsync(id);
        if (entrega == null)
            return BadRequest(ResponseBase<bool>.Fail("Entrega no encontrada"));
        var hoy = ColombiaTime.Today;
        if (entrega.Fecha.Date > hoy || entrega.Fecha.Date < hoy.AddDays(-DiasPorConfirmar))
            return BadRequest(ResponseBase<bool>.Fail(
                $"Esa entrega es de hace más de {DiasPorConfirmar} días: se confirma desde el panel"));

        // Se reusa el caso de uso del panel: una sola regla de confirmación y una sola auditoría.
        // La respuesta es solo ok/error: la tablet NO se entera de la variación (sigue a ciegas).
        var medicion = new MedicionTercero(body.Cantinas.Value, body.SaldoLitros.Value, body.Observacion);
        var result = await _mediator.Send(new ConfirmarEntregaTerceroCommand(id, medicion));
        if (!result.Success)
            return BadRequest(ResponseBase<bool>.Fail(result.Message));
        return Ok(ResponseBase<bool>.Ok(true, result.Message));
    }
}
