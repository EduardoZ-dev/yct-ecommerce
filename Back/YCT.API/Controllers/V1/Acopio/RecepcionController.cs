using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YCT.Application.Common;
using YCT.Application.DTOs;
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

    public RecepcionController(
        IMediator mediator,
        IGenericRepository<Ruta> rutaRepo,
        IGenericRepository<Camion> camionRepo,
        IGenericRepository<Conductor> conductorRepo,
        IGenericRepository<Recogida> recogidaRepo)
    {
        _mediator = mediator;
        _rutaRepo = rutaRepo;
        _camionRepo = camionRepo;
        _conductorRepo = conductorRepo;
        _recogidaRepo = recogidaRepo;
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
        var hoy = DateTime.Now.Date;
        var rutas = (await _rutaRepo.FindAsync(r =>
                r.Status == "EsperandoDescargue" && r.Fecha >= hoy && r.Fecha < hoy.AddDays(1)))
            .OrderBy(r => r.UpdatedAt ?? r.CreatedAt)
            .ToList();

        if (rutas.Count == 0)
            return Ok(ResponseBase<List<RecepcionPendienteDto>>.Ok(new List<RecepcionPendienteDto>()));

        var camiones = (await _camionRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.Nombre);
        var conductores = (await _conductorRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.NombreCompleto);
        var rutaIds = rutas.Select(r => r.Id).ToHashSet();
        var fincasPorRuta = (await _recogidaRepo.FindAsync(r => rutaIds.Contains(r.RutaId)))
            .GroupBy(r => r.RutaId)
            .ToDictionary(g => g.Key, g => g.Count());

        var dtos = rutas.Select(r => new RecepcionPendienteDto
        {
            Id = r.Id,
            Codigo = r.Codigo,
            Fecha = r.Fecha,
            CamionNombre = camiones.TryGetValue(r.CamionId, out var cn) ? cn : $"#{r.CamionId}",
            ConductorNombre = conductores.TryGetValue(r.ConductorId, out var kn) ? kn : $"#{r.ConductorId}",
            NumFincas = fincasPorRuta.TryGetValue(r.Id, out var nf) ? nf : 0,
            EnviadoAt = r.UpdatedAt ?? r.CreatedAt
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
            HoraDescargue = DateTime.Now.TimeOfDay,
            Observaciones = req.Observacion
        });

        // Respuesta neutra: nunca exponemos la diferencia ni lo del chofer a esta tablet.
        if (!result.Success)
            return BadRequest(ResponseBase<object>.Fail(result.Message));
        return Ok(ResponseBase<object>.Ok(new { ok = true }, "Descargue registrado"));
    }
}
