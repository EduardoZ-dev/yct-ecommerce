using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Orders.UpdateStatus;

public record UpdateOrderStatusCommand(int Id, string Status) : IRequest<ResponseBase<OrderDto>>;
