using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.Acopio.ClientesTerceros.ConfirmarEntrega;
using YCT.Application.UseCases.Acopio.ClientesTerceros.Delete;
using YCT.Application.UseCases.Acopio.ClientesTerceros.DeleteEntrega;
using YCT.Application.UseCases.Acopio.ClientesTerceros.GetAll;
using YCT.Application.UseCases.Acopio.ClientesTerceros.GetEntregas;
using YCT.Application.UseCases.Acopio.ClientesTerceros.PromedioCompra;
using YCT.Application.UseCases.Acopio.ClientesTerceros.Save;
using YCT.Application.UseCases.Acopio.ClientesTerceros.SaveEntrega;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1.Acopio;

/// <summary>
/// Clientes terceros (proveedores ocasionales sin ruta fija) y sus entregas de leche.
/// </summary>
[ApiController]
[Route("api/acopio/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class ClientesTercerosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientesTercerosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllClientesTercerosQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> Create([FromBody] SaveClienteTerceroCommand command)
    {
        var result = await _mediator.Send(command with { Id = null });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> Update(int id, [FromBody] SaveClienteTerceroCommand command)
    {
        var result = await _mediator.Send(command with { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.CanDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteClienteTerceroCommand(id));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ===== Entregas =====

    /// <summary>
    /// Entregas de terceros. Con <paramref name="fecha"/> devuelve solo las de ese día:
    /// así la validación del descargue puede mostrar qué leche de terceros entró esa fecha.
    /// </summary>
    [HttpGet("entregas")]
    public async Task<IActionResult> GetEntregas(
        [FromQuery] int dias = GetEntregasTercerosQuery.DiasPorDefecto,
        [FromQuery] int? clienteId = null,
        [FromQuery] DateTime? fecha = null)
    {
        var result = await _mediator.Send(new GetEntregasTercerosQuery(dias, clienteId, fecha));
        return Ok(result);
    }

    /// <summary>Registrar una entrega. Cualquier rol del panel puede hacerlo (el empleado de planta recibe la leche).</summary>
    [HttpPost("{id}/entregas")]
    public async Task<IActionResult> CreateEntrega(int id, [FromBody] SaveEntregaTerceroCommand command)
    {
        var result = await _mediator.Send(command with { ClienteTerceroId = id, Id = null });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Corregir una entrega ya registrada (litros, fecha, precio u observación). Solo administración.</summary>
    [HttpPut("entregas/{entregaId}")]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> UpdateEntrega(int entregaId, [FromBody] SaveEntregaTerceroCommand command)
    {
        var result = await _mediator.Send(command with { Id = entregaId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// A cuánto se ha venido comprando la leche a terceros (promedio ponderado por litros).
    /// Es la referencia que se muestra al poner el precio de una entrega nueva.
    /// </summary>
    [HttpGet("promedio-compra")]
    public async Task<IActionResult> PromedioCompra([FromQuery] int dias = 0)
    {
        var result = await _mediator.Send(new GetPromedioCompraTercerosQuery(dias));
        return Ok(result);
    }

    /// <summary>
    /// Marca que la leche de la entrega llegó a planta. Cualquier rol del panel puede hacerlo
    /// (quien esté recibiendo); es idempotente, así que repetirla no altera la confirmación.
    /// </summary>
    [HttpPost("entregas/{entregaId}/confirmar")]
    public async Task<IActionResult> ConfirmarEntrega(int entregaId)
    {
        var result = await _mediator.Send(new ConfirmarEntregaTerceroCommand(entregaId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("entregas/{entregaId}")]
    [Authorize(Roles = Roles.CanDelete)]
    public async Task<IActionResult> DeleteEntrega(int entregaId)
    {
        var result = await _mediator.Send(new DeleteEntregaTerceroCommand(entregaId));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
