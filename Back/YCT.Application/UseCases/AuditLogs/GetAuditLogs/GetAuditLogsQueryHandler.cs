using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.AuditLogs.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, ResponseBase<AuditLogPageDto>>
{
    private readonly IGenericRepository<AuditLog> _repository;

    public GetAuditLogsQueryHandler(IGenericRepository<AuditLog> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<AuditLogPageDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 10, 200);

        // Repo simple — traemos todo y filtramos en memoria. OK para volúmenes esperados.
        // Cuando crezca podemos cambiar a IQueryable con paginación en SQL.
        var all = await _repository.GetAllAsync();

        var query = all.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(a => a.Action == request.Action);

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId);

        if (request.From.HasValue)
            query = query.Where(a => a.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(a => a.CreatedAt <= request.To.Value);

        if (request.Success.HasValue)
            query = query.Where(a => a.Success == request.Success);

        var ordered = query.OrderByDescending(a => a.CreatedAt).ToList();

        var total = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Username = a.Username,
                UserRole = a.UserRole,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Summary = a.Summary,
                Details = a.Details,
                IpAddress = a.IpAddress,
                Success = a.Success,
                CreatedAt = a.CreatedAt
            }).ToList();

        return ResponseBase<AuditLogPageDto>.Ok(new AuditLogPageDto
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }
}
