using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.Categories.Create;
using YCT.Application.UseCases.Categories.Delete;
using YCT.Application.UseCases.Categories.GetAll;
using YCT.Application.UseCases.Categories.GetById;
using YCT.Application.UseCases.Categories.Update;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryCommand command)
    {
        if (id != command.Id)
            return BadRequest("El ID de la ruta no coincide con el del cuerpo");

        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.CanDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(id));
        return result.Success ? Ok(result) : NotFound(result);
    }
}
