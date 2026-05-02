using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Orders.CreateGuest;

public record CreateGuestOrderCommand(
    string FullName,
    string Phone,
    string ShippingAddress,
    string? Notes,
    List<CreateGuestOrderItem> Items,
    string? ShippingCity = null,
    decimal? ShippingLat = null,
    decimal? ShippingLng = null,
    string PaymentMethod = "OnDelivery") : IRequest<ResponseBase<OrderDto>>;

public record CreateGuestOrderItem(int ProductId, int Quantity);
