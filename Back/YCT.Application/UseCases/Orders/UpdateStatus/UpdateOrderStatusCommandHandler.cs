using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Orders.UpdateStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, ResponseBase<OrderDto>>
{
    private readonly IGenericRepository<Order> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public UpdateOrderStatusCommandHandler(IGenericRepository<Order> repository, IUnitOfWork unitOfWork, IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var validStatuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };
        if (!validStatuses.Contains(request.Status))
            return ResponseBase<OrderDto>.Fail($"Estado inválido. Estados válidos: {string.Join(", ", validStatuses)}");

        var order = await _repository.GetByIdAsync(request.Id);
        if (order == null)
            return ResponseBase<OrderDto>.Fail("Orden no encontrada");

        var previousStatus = order.Status;
        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("StatusChange", "Order", order.Id,
            $"Pedido {order.OrderNumber}: {previousStatus} → {request.Status}",
            new { order.OrderNumber, previousStatus, newStatus = request.Status },
            ct: cancellationToken);

        return ResponseBase<OrderDto>.Ok(new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Total = order.Total,
            Status = order.Status,
            UserId = order.UserId
        }, "Estado de la orden actualizado");
    }
}
