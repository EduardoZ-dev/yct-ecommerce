using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.Acopio.Granjeros.Delete;
using YCT.Application.UseCases.Acopio.Granjeros.GetAll;
using YCT.Application.UseCases.Acopio.Granjeros.Save;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1.Acopio;

[ApiController]
[Route("api/acopio/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class GranjerosController : ControllerBase
{
    private readonly IMediator _mediator;

    public GranjerosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllGranjerosQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> Create([FromBody] SaveGranjeroCommand command)
    {
        var result = await _mediator.Send(command with { Id = null });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> Update(int id, [FromBody] SaveGranjeroCommand command)
    {
        var result = await _mediator.Send(command with { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.CanDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteGranjeroCommand(id));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
