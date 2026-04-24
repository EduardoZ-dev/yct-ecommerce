using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Orders.GetAll;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, ResponseBase<List<OrderDto>>>
{
    private readonly IGenericRepository<Order> _repository;

    public GetAllOrdersQueryHandler(IGenericRepository<Order> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<List<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetAllAsync();
        var dtos = orders.Select(o => new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            OrderDate = o.OrderDate,
            Total = o.Total,
            Status = o.Status,
            Notes = o.Notes,
            ShippingAddress = o.ShippingAddress,
            UserId = o.UserId,
            UserFullName = o.User?.FullName ?? string.Empty,
            Details = o.OrderDetails?.Select(d => new OrderDetailDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = d.Product?.Name ?? string.Empty,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            }).ToList() ?? new()
        }).ToList();

        return ResponseBase<List<OrderDto>>.Ok(dtos);
    }
}
