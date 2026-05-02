using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.Users.Create;
using YCT.Application.UseCases.Users.Delete;
using YCT.Application.UseCases.Users.GetAll;
using YCT.Application.UseCases.Users.Update;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.CanManageUsers)]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllUsersQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.Id) return BadRequest("El ID no coincide");
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteUserCommand(id));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
