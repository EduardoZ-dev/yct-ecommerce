using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Orders.GetAll;

public record GetAllOrdersQuery : IRequest<ResponseBase<List<OrderDto>>>;
