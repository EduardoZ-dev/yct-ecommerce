using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.Distributors.Delete;
using YCT.Application.UseCases.Distributors.GetAll;
using YCT.Application.UseCases.Distributors.Save;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class DistributorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DistributorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllDistributorsQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveDistributorCommand command)
    {
        var result = await _mediator.Send(command with { Id = null });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveDistributorCommand command)
    {
        var result = await _mediator.Send(command with { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.CanDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteDistributorCommand(id));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
