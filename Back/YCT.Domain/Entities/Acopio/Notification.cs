using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Notificación in-app. Generada por eventos del sistema (ej: faltante de leche al validar planta).
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>Tipo: ShortageDetected, ShortageAuthorized, DailyReport, etc.</summary>
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    /// <summary>User destinatario. Null = broadcast a todos los admins.</summary>
    public int? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>Planilla relacionada si aplica.</summary>
    public int? PlanillaId { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}
