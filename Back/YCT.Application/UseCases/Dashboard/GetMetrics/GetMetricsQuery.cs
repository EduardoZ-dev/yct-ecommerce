using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Dashboard.GetMetrics;

public record GetMetricsQuery() : IRequest<ResponseBase<DashboardMetricsDto>>;
