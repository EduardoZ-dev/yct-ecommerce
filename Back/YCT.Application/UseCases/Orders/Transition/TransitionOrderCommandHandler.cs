using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Orders.Transition;

public class TransitionOrderCommandHandler : IRequestHandler<TransitionOrderCommand, ResponseBase<OrderDto>>
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<Distributor> _distributorRepository;
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public TransitionOrderCommandHandler(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<Distributor> distributorRepository,
        IGenericRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _orderRepository = orderRepository;
        _distributorRepository = distributorRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<OrderDto>> Handle(TransitionOrderCommand request, CancellationToken cancellationToken)
    {
        var orderWithDetails = await _orderRepository.FindAsync(o => o.Id == request.Id, o => o.OrderDetails);
        var order = orderWithDetails.FirstOrDefault();
        if (order == null)
            return ResponseBase<OrderDto>.Fail("Pedido no encontrado");

        if (order.Status == "Cancelled" && request.Action != OrderAction.Cancel)
            return ResponseBase<OrderDto>.Fail("Este pedido fue cancelado y no se puede modificar");

        var prevStatus = order.Status;
        string label = "";

        switch (request.Action)
        {
            case OrderAction.Validate:
                if (order.Status != "Pending")
                    return ResponseBase<OrderDto>.Fail("Solo se pueden validar pedidos en estado Pendiente");
                order.ValidatedAt = DateTime.UtcNow;
                label = $"Pedido #{order.Consecutive} validado";
                break;

            case OrderAction.Confirm:
                if (order.Status != "Pending")
                    return ResponseBase<OrderDto>.Fail("Solo se pueden confirmar pedidos en estado Pendiente");
                if (order.ValidatedAt == null) order.ValidatedAt = DateTime.UtcNow;
                order.Status = "Confirmed";
                label = $"Pedido #{order.Consecutive} confirmado";
                break;

            case OrderAction.Ship:
                if (order.Status != "Confirmed")
                    return ResponseBase<OrderDto>.Fail("Solo se pueden enviar pedidos confirmados");

                if (request.DistributorId.HasValue)
                {
                    var distributor = await _distributorRepository.GetByIdAsync(request.DistributorId.Value);
                    if (distributor == null || !distributor.IsActive)
                        return ResponseBase<OrderDto>.Fail("Distribuidor inválido o inactivo");
                    order.DistributorId = distributor.Id;
                }
                else
                {
                    return ResponseBase<OrderDto>.Fail("Debes seleccionar un distribuidor");
                }

                order.TrackingNumber = string.IsNullOrWhiteSpace(request.TrackingNumber) ? null : request.TrackingNumber.Trim();
                order.Status = "Shipped";
                order.ShippedAt = DateTime.UtcNow;
                label = $"Pedido #{order.Consecutive} salió en envío";
                break;

            case OrderAction.Deliver:
                if (order.Status != "Shipped")
                    return ResponseBase<OrderDto>.Fail("Solo se pueden entregar pedidos en envío");
                if (request.CustomerRating.HasValue && (request.CustomerRating < 1 || request.CustomerRating > 5))
                    return ResponseBase<OrderDto>.Fail("La calificación debe estar entre 1 y 5");

                order.Status = "Delivered";
                order.DeliveredAt = DateTime.UtcNow;
                order.CustomerRating = request.CustomerRating;
                order.FeedbackComment = string.IsNullOrWhiteSpace(request.FeedbackComment) ? null : request.FeedbackComment.Trim();

                // Si era pago contra entrega, marcamos como pagado automáticamente
                if (order.PaymentMethod == "OnDelivery" && order.PaymentStatus == "Unpaid")
                {
                    order.PaymentStatus = "Paid";
                    order.PaidAt = DateTime.UtcNow;
                }
                label = $"Pedido #{order.Consecutive} entregado";
                break;

            case OrderAction.Cancel:
                if (order.Status == "Delivered")
                    return ResponseBase<OrderDto>.Fail("No se puede cancelar un pedido entregado");
                if (order.Status == "Cancelled")
                    return ResponseBase<OrderDto>.Fail("Este pedido ya está cancelado");

                // Restaurar stock de los productos del pedido
                var restoredItems = new List<object>();
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _productRepository.GetByIdAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.Stock += detail.Quantity;
                        await _productRepository.UpdateAsync(product);
                        restoredItems.Add(new { product.Id, product.Name, restored = detail.Quantity, newStock = product.Stock });
                    }
                }

                order.Status = "Cancelled";
                label = $"Pedido #{order.Consecutive} cancelado · stock restaurado para {restoredItems.Count} producto(s)";
                break;
        }

        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync($"Transition_{request.Action}", "Order", order.Id,
            label,
            new { prevStatus, newStatus = order.Status, request.DistributorId, request.TrackingNumber, request.CustomerRating },
            ct: cancellationToken);

        return ResponseBase<OrderDto>.Ok(MapToDto(order), label);
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        Consecutive = o.Consecutive,
        OrderDate = o.OrderDate,
        Total = o.Total,
        Status = o.Status,
        PaymentMethod = o.PaymentMethod,
        PaymentStatus = o.PaymentStatus,
        PaidAt = o.PaidAt,
        Notes = o.Notes,
        ShippingAddress = o.ShippingAddress,
        ShippingCity = o.ShippingCity,
        ShippingLat = o.ShippingLat,
        ShippingLng = o.ShippingLng,
        UserId = o.UserId
    };
}
