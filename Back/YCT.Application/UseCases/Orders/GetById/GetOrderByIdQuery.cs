using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Orders.GetById;

public record GetOrderByIdQuery(int Id) : IRequest<ResponseBase<OrderDto>>;
