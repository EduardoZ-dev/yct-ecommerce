using YCT.Domain.Common;

namespace YCT.Domain.Entities;

/// <summary>
/// Registro de auditoría de acciones administrativas.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>Quien realizó la acción (null = anónimo / login fallido)</summary>
    public int? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>Para mostrar aunque el usuario sea borrado</summary>
    public string Username { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;

    /// <summary>Tipo de acción: Login, Logout, LoginFailed, Create, Update, Delete, StatusChange…</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Tipo de entidad afectada: Product, Category, Order, User, Auth…</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Id de la entidad afectada (null si no aplica)</summary>
    public int? EntityId { get; set; }

    /// <summary>Resumen legible de lo que pasó</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>JSON con detalles (antes/después, payload, errores…)</summary>
    public string? Details { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    /// <summary>true = acción exitosa, false = falló</summary>
    public bool Success { get; set; } = true;
}
