using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.Orders.Create;
using YCT.Application.UseCases.Orders.CreateGuest;
using YCT.Application.UseCases.Orders.GetAll;
using YCT.Application.UseCases.Orders.GetById;
using YCT.Application.UseCases.Orders.Track;
using YCT.Application.UseCases.Orders.Transition;
using YCT.Application.UseCases.Orders.UpdatePayment;
using YCT.Application.UseCases.Orders.UpdateStatus;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllOrdersQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var result = await _mediator.Send(new UpdateOrderStatusCommand(id, request.Status));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id}/payment")]
    public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentRequest request)
    {
        var result = await _mediator.Send(new UpdatePaymentCommand(id, request.PaymentStatus, request.PaymentMethod));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Marca el pedido como validado (paso 2 del wizard).</summary>
    [HttpPatch("{id}/validate")]
    public async Task<IActionResult> Validate(int id)
    {
        var result = await _mediator.Send(new TransitionOrderCommand(id, OrderAction.Validate));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Confirma el pedido (paso 3 del wizard).</summary>
    [HttpPatch("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var result = await _mediator.Send(new TransitionOrderCommand(id, OrderAction.Confirm));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Marca el pedido como enviado y asigna distribuidor (paso 4).</summary>
    [HttpPatch("{id}/ship")]
    public async Task<IActionResult> Ship(int id, [FromBody] ShipOrderRequest request)
    {
        var result = await _mediator.Send(new TransitionOrderCommand(
            id, OrderAction.Ship,
            DistributorId: request.DistributorId,
            TrackingNumber: request.TrackingNumber));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Marca el pedido como entregado con feedback opcional (paso 5).</summary>
    [HttpPatch("{id}/deliver")]
    public async Task<IActionResult> Deliver(int id, [FromBody] DeliverOrderRequest request)
    {
        var result = await _mediator.Send(new TransitionOrderCommand(
            id, OrderAction.Deliver,
            CustomerRating: request.CustomerRating,
            FeedbackComment: request.FeedbackComment));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Cancela el pedido en cualquier momento antes de la entrega.</summary>
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _mediator.Send(new TransitionOrderCommand(id, OrderAction.Cancel));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Permite que un cliente sin cuenta cree un pedido (checkout público).</summary>
    [HttpPost("guest")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateGuest([FromBody] CreateGuestOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Permite consultar el estado de un pedido por número de orden o teléfono.</summary>
    [HttpGet("track")]
    [AllowAnonymous]
    public async Task<IActionResult> Track([FromQuery] string search)
    {
        var result = await _mediator.Send(new TrackOrderQuery(search));
        return Ok(result);
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class UpdatePaymentRequest
{
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
}

public class ShipOrderRequest
{
    public int DistributorId { get; set; }
    public string? TrackingNumber { get; set; }
}

public class DeliverOrderRequest
{
    public int? CustomerRating { get; set; }
    public string? FeedbackComment { get; set; }
}
