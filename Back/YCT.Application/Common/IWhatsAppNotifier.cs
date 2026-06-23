namespace YCT.Application.Common;

/// <summary>
/// Envía notificaciones por WhatsApp (API oficial de Meta) a los contactos configurados.
/// Best-effort: nunca debe romper el flujo de negocio si falla.
/// </summary>
public interface IWhatsAppNotifier
{
    /// <summary>Reporte de un descargue (salga bien o con faltante) a todos los destinatarios.</summary>
    Task SendDescargueAsync(WhatsAppDescargueModel model, CancellationToken cancellationToken = default);
}

public record WhatsAppDescargueModel(
    string Resultado,        // "OK" | "CON FALTANTE"
    string Codigo,
    DateTime Fecha,
    string Conductor,
    string Camion,
    decimal LitrosChofer,
    decimal LitrosPlanta,
    decimal Diferencia,
    string Estado,
    string HistorialUrl);
