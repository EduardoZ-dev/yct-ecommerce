using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.Acopio.Tinas.GetAll;
using YCT.Application.UseCases.Acopio.Tinas.UpdateFinca;
using YCT.Application.UseCases.Acopio.Tinas.UpdatePlanta;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1.Acopio;

[ApiController]
[Route("api/acopio/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class TinasController : ControllerBase
{
    private readonly IMediator _mediator;

    public TinasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Fincas con sus tinas, tinas de planta e historial de ajustes.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetTinasQuery());
        return Ok(result);
    }

    public record UpdateFincaBody(int Cantidad, string? Observacion);
    public record UpdatePlantaBody(int Cantidad, string? Observacion);

    /// <summary>Ajusta las tinas de YCT en una finca (código).</summary>
    [HttpPut("finca/{codigoId}")]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> UpdateFinca(int codigoId, [FromBody] UpdateFincaBody body)
    {
        var result = await _mediator.Send(new UpdateTinaFincaCommand(codigoId, body.Cantidad, body.Observacion));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Ajusta las tinas de YCT en la planta y/o agrega una observación.</summary>
    [HttpPut("planta")]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> UpdatePlanta([FromBody] UpdatePlantaBody body)
    {
        var result = await _mediator.Send(new UpdateTinaPlantaCommand(body.Cantidad, body.Observacion));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
