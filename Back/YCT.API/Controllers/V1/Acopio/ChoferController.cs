using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Application.UseCases.Acopio.Planillas.Save;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.API.Controllers.V1.Acopio;

/// <summary>
/// Endpoints expuestos a la app móvil del chofer (PWA YCT Chofer).
/// TODO: agregar auth con JWT rol Conductor + PIN login. Por ahora AllowAnonymous para MVP demo.
/// </summary>
[ApiController]
[Route("api/acopio/chofer")]
[AllowAnonymous]
public class ChoferController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IGenericRepository<Ruta> _rutaRepo;
    private readonly IGenericRepository<Camion> _camionRepo;

    public ChoferController(
        IMediator mediator,
        IGenericRepository<Ruta> rutaRepo,
        IGenericRepository<Camion> camionRepo)
    {
        _mediator = mediator;
        _rutaRepo = rutaRepo;
        _camionRepo = camionRepo;
    }

    /// <summary>Recibe la planilla enviada por el chofer desde la app móvil.</summary>
    [HttpPost("recogidas")]
    public async Task<IActionResult> EnviarRecogidas([FromBody] ChoferEnvioRequest req)
    {
        if (req.Recogidas == null || req.Recogidas.Count == 0)
            return BadRequest(ResponseBase<object>.Fail("No hay recogidas en el envío"));

        // Generar código (CAMION-FECHA-HHMM) si no existe ya una ruta del día/camión/conductor
        var camion = await _camionRepo.GetByIdAsync(req.CamionId);
        if (camion == null) return BadRequest(ResponseBase<object>.Fail("Camión no encontrado"));

        var fecha = req.Fecha.Date;
        // Buscar ruta del día NO finalizada (Conciliada/Anulada/PendienteAutorizacion → crear nueva)
        var existing = (await _rutaRepo.FindAsync(r =>
            r.Fecha == fecha && r.CamionId == req.CamionId && r.ConductorId == req.ConductorId
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
                r.Fecha == fecha && r.CamionId == req.CamionId && r.ConductorId == req.ConductorId)).ToList();
            var baseCodigo = $"{camion.Nombre}-{fecha:ddMM}";
            codigo = rutasDelDia.Count == 0 ? baseCodigo : $"{baseCodigo}-{rutasDelDia.Count + 1}";
        }

        var saveCmd = new SavePlanillaCommand(
            existing?.Id,
            codigo,
            fecha,
            req.CamionId,
            req.ConductorId,
            req.AsistenteId,
            req.HoraSalida,
            null, // HoraLlegada → la registra el operario planta
            null, // HoraDescargue → idem
            $"[Enviado por chofer · UUID {req.Uuid}]",
            req.Recogidas.Select(r => new SavePlanillaItemRequest
            {
                Id = null,
                GranjeroId = r.GranjeroId,
                Fecha = fecha,
                Cantinas = r.Cantinas,
                SaldoLitros = r.SaldoLitros
            }).ToList()
        );

        var result = await _mediator.Send(saveCmd);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
