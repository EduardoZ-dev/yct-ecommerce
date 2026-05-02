using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Orders.UpdatePayment;

public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, ResponseBase<OrderDto>>
{
    private readonly IGenericRepository<Order> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public UpdatePaymentCommandHandler(
        IGenericRepository<Order> repository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<OrderDto>> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var validStatus = new[] { "Unpaid", "Paid", "Refunded" };
        if (!validStatus.Contains(request.PaymentStatus))
            return ResponseBase<OrderDto>.Fail($"Estado de pago inválido. Válidos: {string.Join(", ", validStatus)}");

        var order = await _repository.GetByIdAsync(request.Id);
        if (order == null)
            return ResponseBase<OrderDto>.Fail("Orden no encontrada");

        var prevStatus = order.PaymentStatus;
        var prevMethod = order.PaymentMethod;

        order.PaymentStatus = request.PaymentStatus;
        if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
            order.PaymentMethod = request.PaymentMethod;

        if (request.PaymentStatus == "Paid" && order.PaidAt == null)
            order.PaidAt = DateTime.UtcNow;
        else if (request.PaymentStatus != "Paid")
            order.PaidAt = null;

        order.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("PaymentChange", "Order", order.Id,
            $"Pago actualizado en pedido #{order.Consecutive}: {prevStatus} → {request.PaymentStatus}",
            new { order.OrderNumber, prevStatus, newStatus = request.PaymentStatus, prevMethod, newMethod = order.PaymentMethod },
            ct: cancellationToken);

        return ResponseBase<OrderDto>.Ok(new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Consecutive = order.Consecutive,
            OrderDate = order.OrderDate,
            Total = order.Total,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            PaidAt = order.PaidAt,
            UserId = order.UserId
        }, "Estado de pago actualizado");
    }
}
