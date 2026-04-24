using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Orders.Create;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ResponseBase<OrderDto>>
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseBase<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            UserId = request.UserId,
            Notes = request.Notes,
            ShippingAddress = request.ShippingAddress,
            Status = "Pending"
        };

        decimal total = 0;
        foreach (var detail in request.Details)
        {
            var product = await _productRepository.GetByIdAsync(detail.ProductId);
            if (product == null)
                return ResponseBase<OrderDto>.Fail($"Producto con ID {detail.ProductId} no encontrado");

            if (product.Stock < detail.Quantity)
                return ResponseBase<OrderDto>.Fail($"Stock insuficiente para '{product.Name}'. Disponible: {product.Stock}");

            var subtotal = product.Price * detail.Quantity;
            total += subtotal;

            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                UnitPrice = product.Price,
                Subtotal = subtotal
            });

            product.Stock -= detail.Quantity;
            await _productRepository.UpdateAsync(product);
        }

        order.Total = total;
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ResponseBase<OrderDto>.Ok(new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Total = order.Total,
            Status = order.Status,
            Notes = order.Notes,
            ShippingAddress = order.ShippingAddress,
            UserId = order.UserId,
            Details = order.OrderDetails.Select(d => new OrderDetailDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            }).ToList()
        }, "Orden creada exitosamente");
    }
}
