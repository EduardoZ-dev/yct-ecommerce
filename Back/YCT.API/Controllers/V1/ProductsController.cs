using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.Products.Create;
using YCT.Application.UseCases.Products.Delete;
using YCT.Application.UseCases.Products.GetAll;
using YCT.Application.UseCases.Products.GetById;
using YCT.Application.UseCases.Products.GetCatalog;
using YCT.Application.UseCases.Products.Reorder;
using YCT.Application.UseCases.Products.Update;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog()
    {
        var result = await _mediator.Send(new GetCatalogQuery());
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command)
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
        var result = await _mediator.Send(new DeleteProductCommand(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderProductsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
