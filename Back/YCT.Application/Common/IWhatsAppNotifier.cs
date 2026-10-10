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

    /// <summary>Entrada de cliente tercero confirmada en planta, con lo registrado vs lo recibido.</summary>
    Task SendTerceroAsync(WhatsAppTerceroModel model, CancellationToken cancellationToken = default);
}

/// <summary>
/// Llegada de leche de un tercero. Plantilla de Meta: WhatsApp:TemplateTercero. El enlace al panel
/// lo arma el notificador con AppUrls:Admin (configuración de infraestructura, no del caso de uso).
/// </summary>
public record WhatsAppTerceroModel(
    string Resultado,          // "✅ Llegó completa" | "🚨 Llegó MENOS leche" | "⬆️ Llegó MÁS leche"
    string Cliente,
    DateTime Fecha,            // día de la entrega
    decimal LitrosRegistrados, // lo que registró acopio en el panel
    decimal LitrosRecibidos,   // lo que midió el receptor, a ciegas
    decimal Diferencia,        // recibidos − registrados
    string Observacion,        // nota del receptor; vacía si no escribió nada
    string RecibidoPor);

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
    string FincasDetalle,    // "La Esperanza: 320 L · El Roble: 210 L" (una sola línea, sin saltos)
    string HistorialUrl);
