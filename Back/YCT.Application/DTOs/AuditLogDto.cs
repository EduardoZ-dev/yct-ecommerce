namespace YCT.Application.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public bool Success { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuditLogPageDto
{
    public List<AuditLogDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
