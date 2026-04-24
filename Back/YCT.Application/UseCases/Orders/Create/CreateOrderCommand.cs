using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Orders.Create;

public record CreateOrderCommand(
    int UserId,
    string? Notes,
    string? ShippingAddress,
    List<CreateOrderDetailCommand> Details) : IRequest<ResponseBase<OrderDto>>;

public record CreateOrderDetailCommand(int ProductId, int Quantity);
