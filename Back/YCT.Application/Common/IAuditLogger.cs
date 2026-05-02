namespace YCT.Application.Common;

public interface IAuditLogger
{
    Task LogAsync(
        string action,
        string entityType,
        int? entityId,
        string summary,
        object? details = null,
        bool success = true,
        int? overrideUserId = null,
        string? overrideUsername = null,
        CancellationToken ct = default);
}
