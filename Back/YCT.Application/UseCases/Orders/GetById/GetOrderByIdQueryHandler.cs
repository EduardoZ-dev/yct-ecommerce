using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Orders.GetById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, ResponseBase<OrderDto>>
{
    private readonly IGenericRepository<Order> _repository;

    public GetOrderByIdQueryHandler(IGenericRepository<Order> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.Id);
        if (order == null)
            return ResponseBase<OrderDto>.Fail("Orden no encontrada");

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
            UserFullName = order.User?.FullName ?? string.Empty,
            Details = order.OrderDetails?.Select(d => new OrderDetailDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = d.Product?.Name ?? string.Empty,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            }).ToList() ?? new()
        });
    }
}
