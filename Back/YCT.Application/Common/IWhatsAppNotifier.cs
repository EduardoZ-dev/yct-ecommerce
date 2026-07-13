namespace YCT.Application.Common;

/// <summary>
/// Envía notificaciones por WhatsApp (API oficial de Meta) a los contactos configurados.
/// Best-effort: nunca debe romper el flujo de negocio si falla.
/// </summary>
public interface IWhatsAppNotifier
{
    /// <summary>Reporte de un descargue (salga bien o con faltante) a todos los destinatarios.</summary>
    Task SendDescargueAsync(WhatsAppDescargueModel model, CancellationToken cancellationToken = default);

    /// <summary>Novedad reportada por un chofer en plena ruta (llanta, trancón, finca sin ordeño…).</summary>
    Task SendNovedadAsync(WhatsAppNovedadModel model, CancellationToken cancellationToken = default);
}

/// <summary>Aviso de novedad en ruta. Plantilla de Meta: WhatsApp:TemplateNovedad.</summary>
public record WhatsAppNovedadModel(
    string Tipo,          // "Llanta averiada"
    string Categoria,     // "Camión" | "Vía" | "Finca" | "Otro"
    string Conductor,
    string Camion,
    DateTime ReportadoAt,
    string Detalle,       // descripción libre o "-"
    string Finca);        // finca afectada o "-"

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
