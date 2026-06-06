using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.Acopio.Camiones.Delete;
using YCT.Application.UseCases.Acopio.Camiones.GetAll;
using YCT.Application.UseCases.Acopio.Camiones.Save;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1.Acopio;

[ApiController]
[Route("api/acopio/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class CamionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CamionesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllCamionesQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> Create([FromBody] SaveCamionCommand command)
    {
        var result = await _mediator.Send(command with { Id = null });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> Update(int id, [FromBody] SaveCamionCommand command)
    {
        var result = await _mediator.Send(command with { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.CanDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCamionCommand(id));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
