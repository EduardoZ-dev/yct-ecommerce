using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.AuditLogs.GetAuditLogs;

public record GetAuditLogsQuery(
    int Page = 1,
    int PageSize = 50,
    string? Action = null,
    string? EntityType = null,
    int? UserId = null,
    DateTime? From = null,
    DateTime? To = null,
    bool? Success = null
) : IRequest<ResponseBase<AuditLogPageDto>>;
