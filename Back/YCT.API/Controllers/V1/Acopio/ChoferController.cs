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

    public ChoferController(
        IMediator mediator,
        IGenericRepository<Ruta> rutaRepo,
        IGenericRepository<Camion> camionRepo,
        IGenericRepository<Granjero> granjeroRepo,
        IGenericRepository<GranjeroCodigo> codigoRepo)
    {
        _mediator = mediator;
        _rutaRepo = rutaRepo;
        _camionRepo = camionRepo;
        _granjeroRepo = granjeroRepo;
        _codigoRepo = codigoRepo;
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
                    .Select(c => new { id = c.Id, codigo = c.Codigo, finca = c.Finca })
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
        // Buscar ruta del día NO finalizada (Conciliada/Anulada/PendienteAutorizacion → crear nueva)
        var existing = (await _rutaRepo.FindAsync(r =>
            r.Fecha == fecha && r.CamionId == req.CamionId && r.ConductorId == conductorId
            && r.Status != "Conciliada" && r.Status != "Anulada" && r.Status != "PendienteAutorizacion"))
            .FirstOrDefault();

        string codigo;
        if (existing != null)
        {
            codigo = existing.Codigo;
        }
        else
        {
            // Si ya hay rutas del día (finalizadas), generar código con sufijo incremental
            var rutasDelDia = (await _rutaRepo.FindAsync(r =>
                r.Fecha == fecha && r.CamionId == req.CamionId && r.ConductorId == conductorId)).ToList();
            var baseCodigo = $"{camion.Nombre}-{fecha:ddMM}";
            codigo = rutasDelDia.Count == 0 ? baseCodigo : $"{baseCodigo}-{rutasDelDia.Count + 1}";
        }

        var saveCmd = new SavePlanillaCommand(
            existing?.Id,
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
            }).ToList()
        );

        var result = await _mediator.Send(saveCmd);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
