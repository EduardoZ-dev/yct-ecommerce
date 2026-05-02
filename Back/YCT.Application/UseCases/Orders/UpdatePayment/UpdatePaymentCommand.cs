using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Orders.UpdatePayment;

public record UpdatePaymentCommand(int Id, string PaymentStatus, string? PaymentMethod = null) : IRequest<ResponseBase<OrderDto>>;
