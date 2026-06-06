using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.Acopio.Asistentes.Delete;
using YCT.Application.UseCases.Acopio.Asistentes.GetAll;
using YCT.Application.UseCases.Acopio.Asistentes.Save;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1.Acopio;

[ApiController]
[Route("api/acopio/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class AsistentesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AsistentesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllAsistentesQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> Create([FromBody] SaveAsistenteCommand command)
    {
        var result = await _mediator.Send(command with { Id = null });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> Update(int id, [FromBody] SaveAsistenteCommand command)
    {
        var result = await _mediator.Send(command with { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.CanDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteAsistenteCommand(id));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
