using System.Text.Json;
using YCT.Application.Common;
using YCT.Domain.Entities;
using YCT.Infrastructure.Persistence;

namespace YCT.Infrastructure.Services;

public class AuditLogger : IAuditLogger
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditLogger(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task LogAsync(
        string action,
        string entityType,
        int? entityId,
        string summary,
        object? details = null,
        bool success = true,
        int? overrideUserId = null,
        string? overrideUsername = null,
        CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            UserId = overrideUserId ?? _currentUser.UserId,
            Username = overrideUsername ?? _currentUser.Username ?? "anonymous",
            UserRole = _currentUser.Role ?? "Anonymous",
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Summary = summary[..Math.Min(summary.Length, 500)],
            Details = details is null ? null : JsonSerializer.Serialize(details, JsonOpts),
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent,
            Success = success,
            CreatedAt = DateTime.UtcNow
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(ct);
    }
}
