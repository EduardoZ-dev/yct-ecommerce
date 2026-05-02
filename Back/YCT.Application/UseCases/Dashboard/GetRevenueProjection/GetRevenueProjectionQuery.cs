using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Dashboard.GetRevenueProjection;

public record GetRevenueProjectionQuery() : IRequest<ResponseBase<RevenueProjectionDto>>;
