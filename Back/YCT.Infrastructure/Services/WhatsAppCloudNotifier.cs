using System.Net.Http.Json;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YCT.Application.Common;

namespace YCT.Infrastructure.Services;

/// <summary>
/// Notificador de WhatsApp usando la API oficial (Meta Cloud API).
/// Manda un mensaje de plantilla aprobada a cada destinatario configurado.
///
/// Config (appsettings / variables de entorno):
///   WhatsApp:Enabled        = true|false
///   WhatsApp:Token          = token permanente de la app de Meta
///   WhatsApp:PhoneNumberId  = id del número remitente (WhatsApp Business)
///   WhatsApp:Template       = nombre de la plantilla aprobada (ej. reporte_descargue)
///   WhatsApp:Lang           = idioma de la plantilla (ej. es)
///   WhatsApp:GraphVersion   = versión del Graph API (ej. v21.0)
///   WhatsApp:Recipients:0   = +57300...  (uno por destinatario, hasta los 6)
/// </summary>
public class WhatsAppCloudNotifier : IWhatsAppNotifier
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<WhatsAppCloudNotifier> _logger;

    public WhatsAppCloudNotifier(HttpClient http, IConfiguration config, ILogger<WhatsAppCloudNotifier> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public Task SendDescargueAsync(WhatsAppDescargueModel m, CancellationToken cancellationToken = default)
    {
        // Parámetros de la plantilla (deben coincidir con el orden {{1}}..{{10}} aprobado en Meta).
        string C(decimal v) => v.ToString("0.##", CultureInfo.InvariantCulture);
        var parametros = new[]
        {
            m.Resultado,
            m.Codigo,
            m.Fecha.ToString("dd/MM/yyyy"),
            m.Conductor,
            m.Camion,
            C(m.LitrosChofer),
            C(m.LitrosPlanta),
            C(m.Diferencia),
            m.Estado,
            m.HistorialUrl,
        };

        var template = _config["WhatsApp:Template"] ?? "reporte_descargue";
        return EnviarPlantillaAsync(template, parametros, cancellationToken);
    }

    public Task SendNovedadAsync(WhatsAppNovedadModel m, CancellationToken cancellationToken = default)
    {
        // Orden {{1}}..{{7}} de la plantilla de novedades aprobada en Meta.
        var parametros = new[]
        {
            m.Tipo,
            m.Categoria,
            m.Conductor,
            m.Camion,
            m.ReportadoAt.ToString("dd/MM/yyyy HH:mm"),
            string.IsNullOrWhiteSpace(m.Detalle) ? "-" : m.Detalle,
            string.IsNullOrWhiteSpace(m.Finca) ? "-" : m.Finca,
        };

        var template = _config["WhatsApp:TemplateNovedad"] ?? "novedad_ruta";
        return EnviarPlantillaAsync(template, parametros, cancellationToken);
    }

    /// <summary>
    /// Envío común a todos los destinatarios. Best-effort: si no está configurado o falla,
    /// solo queda en el log — un aviso nunca puede tumbar el flujo de negocio.
    /// </summary>
    private async Task EnviarPlantillaAsync(string template, string[] parametros, CancellationToken cancellationToken)
    {
        if (!_config.GetValue("WhatsApp:Enabled", false))
            return; // desactivado hasta tener credenciales de Meta

        var token = _config["WhatsApp:Token"];
        var phoneNumberId = _config["WhatsApp:PhoneNumberId"];
        var lang = _config["WhatsApp:Lang"] ?? "es";
        var graph = _config["WhatsApp:GraphVersion"] ?? "v21.0";
        var recipients = _config.GetSection("WhatsApp:Recipients").Get<string[]>() ?? Array.Empty<string>();

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(phoneNumberId) || recipients.Length == 0)
        {
            _logger.LogWarning("WhatsApp habilitado pero falta Token/PhoneNumberId/Recipients. No se envía.");
            return;
        }

        var url = $"https://graph.facebook.com/{graph}/{phoneNumberId}/messages";

        foreach (var to in recipients)
        {
            if (string.IsNullOrWhiteSpace(to)) continue;
            try
            {
                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = to.Trim(),
                    type = "template",
                    template = new
                    {
                        name = template,
                        language = new { code = lang },
                        components = new[]
                        {
                            new
                            {
                                type = "body",
                                parameters = parametros.Select(p => new { type = "text", text = p }).ToArray()
                            }
                        }
                    }
                };

                using var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(payload)
                };
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var resp = await _http.SendAsync(req, cancellationToken);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("WhatsApp a {To} falló ({Status}): {Body}", to, (int)resp.StatusCode, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enviando WhatsApp a {To}", to);
            }
        }
    }
}
