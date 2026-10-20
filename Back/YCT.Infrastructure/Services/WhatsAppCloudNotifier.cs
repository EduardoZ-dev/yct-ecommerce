using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YCT.Application.Common;
using YCT.Infrastructure.Services.Tarjetas;

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
///   WhatsApp:TemplateNovedad / WhatsApp:TemplateTercero = plantillas de novedades y de terceros
///   WhatsApp:TemplateDescargueImagen / WhatsApp:TemplateTerceroImagen = plantillas con la TARJETA
///       (imagen) arriba. Vacías = no se usan y sale el texto de siempre.
///   WhatsApp:Lang           = idioma de la plantilla (ej. es)
///   WhatsApp:GraphVersion   = versión del Graph API (ej. v21.0)
///   WhatsApp:GraphBaseUrl   = solo para pruebas locales (por defecto https://graph.facebook.com)
///   WhatsApp:Recipients:0   = +57300...  (uno por destinatario, hasta los 6)
/// </summary>
public partial class WhatsAppCloudNotifier : IWhatsAppNotifier
{
    private const int MaxNombre = 120;
    private const int MaxObservacion = 300;
    private const string RutaClientesTerceros = "clientes-terceros";

    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<WhatsAppCloudNotifier> _logger;
    private readonly ITarjetasWhatsApp _tarjetas;

    public WhatsAppCloudNotifier(HttpClient http, IConfiguration config, ILogger<WhatsAppCloudNotifier> logger,
        ITarjetasWhatsApp tarjetas)
    {
        _http = http;
        _config = config;
        _logger = logger;
        _tarjetas = tarjetas;
    }

    public Task SendDescargueAsync(WhatsAppDescargueModel m, CancellationToken cancellationToken = default)
    {
        // Parámetros de la plantilla (deben coincidir con el orden {{1}}..{{11}} aprobado en Meta).
        var texto = new Plantilla(_config["WhatsApp:Template"] ?? "reporte_descargue", new[]
        {
            m.Resultado,
            m.Codigo,
            Dia(m.Fecha),
            m.Conductor,
            m.Camion,
            Litros(m.LitrosChofer),
            Litros(m.LitrosPlanta),
            Litros(m.Diferencia),
            m.Estado,
            m.FincasDetalle,
            m.HistorialUrl,
        });

        // Con tarjeta: {{1}} resultado · {{2}} planilla · {{3}} planta · {{4}} chofer · {{5}} diferencia.
        var conTarjeta = new PlantillaConTarjeta(_config["WhatsApp:TemplateDescargueImagen"],
            () => m.Aportes is null ? null : _tarjetas.Descargue(m),
            new[] { m.Resultado, m.Codigo, Litros(m.LitrosPlanta), Litros(m.LitrosChofer), Litros(m.Diferencia) });

        return EnviarAsync(texto, conTarjeta, cancellationToken);
    }

    public Task SendNovedadAsync(WhatsAppNovedadModel m, CancellationToken cancellationToken = default)
    {
        // Orden {{1}}..{{7}} de la plantilla de novedades aprobada en Meta.
        var texto = new Plantilla(_config["WhatsApp:TemplateNovedad"] ?? "novedad_ruta", new[]
        {
            m.Tipo,
            m.Categoria,
            m.Conductor,
            m.Camion,
            m.ReportadoAt.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
            string.IsNullOrWhiteSpace(m.Detalle) ? "-" : m.Detalle,
            string.IsNullOrWhiteSpace(m.Finca) ? "-" : m.Finca,
        });
        return EnviarAsync(texto, conTarjeta: null, cancellationToken);
    }

    public Task SendTerceroAsync(WhatsAppTerceroModel m, CancellationToken cancellationToken = default)
    {
        // Sin la URL del panel no se manda: un enlace a ningún lado (o a localhost) confunde más que ayuda.
        var panel = _config["AppUrls:Admin"];
        if (string.IsNullOrWhiteSpace(panel))
        {
            _logger.LogWarning("Falta AppUrls:Admin: no se envía el WhatsApp de la entrada de tercero de {Cliente}.", m.Cliente);
            return Task.CompletedTask;
        }

        // Orden {{1}}..{{9}} de la plantilla reporte_tercero aprobada en Meta.
        var cliente = TextoDeVariable(m.Cliente, MaxNombre);
        var texto = new Plantilla(_config["WhatsApp:TemplateTercero"] ?? "reporte_tercero", new[]
        {
            m.Resultado,
            cliente,
            Dia(m.Fecha),
            Litros(m.LitrosRegistrados),
            Litros(m.LitrosRecibidos),
            m.Diferencia > 0 ? $"+{Litros(m.Diferencia)}" : Litros(m.Diferencia),
            string.IsNullOrWhiteSpace(m.Observacion) ? "Sin observaciones" : TextoDeVariable(m.Observacion, MaxObservacion),
            TextoDeVariable(m.RecibidoPor, MaxNombre),
            $"{panel.TrimEnd('/')}/{RutaClientesTerceros}",
        });

        // Con tarjeta: {{1}} resultado · {{2}} cliente · {{3}} recibido · {{4}} registrado.
        var conTarjeta = new PlantillaConTarjeta(_config["WhatsApp:TemplateTerceroImagen"],
            () => _tarjetas.Tercero(m),
            new[] { m.Resultado, cliente, Litros(m.LitrosRecibidos), Litros(m.LitrosRegistrados) });

        return EnviarAsync(texto, conTarjeta, cancellationToken);
    }

    // ===== Envío =====

    /// <summary>Plantilla de solo texto: nombre y parámetros del cuerpo en orden.</summary>
    private sealed record Plantilla(string Nombre, string[] Parametros);

    /// <summary>Plantilla con la tarjeta (imagen) en el encabezado. Sin nombre configurado no se usa.</summary>
    private sealed record PlantillaConTarjeta(string? Nombre, Func<byte[]?> Dibujar, string[] Parametros);

    private sealed record ConfiguracionEnvio(string Token, string PhoneNumberId, string Idioma, string Graph, string[] Destinatarios, string BaseUrl)
    {
        public string Url(string recurso) => $"{BaseUrl}/{Graph}/{PhoneNumberId}/{recurso}";
    }

    private sealed record MediaSubida([property: JsonPropertyName("id")] string? Id);

    /// <summary>
    /// Envío común a todos los destinatarios. Best-effort: si no está configurado o falla, solo queda
    /// en el log — un aviso nunca puede tumbar el flujo de negocio. Con tarjeta, cada destinatario
    /// que no la reciba (dibujo, subida o plantilla fallidos) recibe el texto de siempre.
    /// </summary>
    private async Task EnviarAsync(Plantilla texto, PlantillaConTarjeta? conTarjeta, CancellationToken cancellationToken)
    {
        var cfg = LeerConfiguracion();
        if (cfg is null)
            return;

        var mediaId = await PrepararTarjetaAsync(cfg, conTarjeta, cancellationToken);
        foreach (var destinatario in cfg.Destinatarios)
        {
            if (mediaId is not null &&
                await EnviarAUnoAsync(cfg, destinatario, conTarjeta!.Nombre!, ComponentesConTarjeta(mediaId, conTarjeta.Parametros), cancellationToken))
                continue;
            await EnviarAUnoAsync(cfg, destinatario, texto.Nombre, ComponentesTexto(texto.Parametros), cancellationToken);
        }
    }

    private ConfiguracionEnvio? LeerConfiguracion()
    {
        if (!_config.GetValue("WhatsApp:Enabled", false))
            return null; // desactivado hasta tener credenciales de Meta

        var token = _config["WhatsApp:Token"];
        var phoneNumberId = _config["WhatsApp:PhoneNumberId"];
        var destinatarios = (_config.GetSection("WhatsApp:Recipients").Get<string[]>() ?? Array.Empty<string>())
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d.Trim())
            .ToArray();
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(phoneNumberId) || destinatarios.Length == 0)
        {
            _logger.LogWarning("WhatsApp habilitado pero falta Token/PhoneNumberId/Recipients. No se envía.");
            return null;
        }
        return new ConfiguracionEnvio(token, phoneNumberId,
            _config["WhatsApp:Lang"] ?? "es", _config["WhatsApp:GraphVersion"] ?? "v21.0", destinatarios,
            (_config["WhatsApp:GraphBaseUrl"] ?? "https://graph.facebook.com").TrimEnd('/'));
    }

    /// <summary>Dibuja la tarjeta y la sube a Meta una sola vez. Null = se manda el texto de siempre.</summary>
    private async Task<string?> PrepararTarjetaAsync(ConfiguracionEnvio cfg, PlantillaConTarjeta? conTarjeta, CancellationToken cancellationToken)
    {
        if (conTarjeta is null || string.IsNullOrWhiteSpace(conTarjeta.Nombre))
            return null;
        try
        {
            var png = conTarjeta.Dibujar();
            return png is null ? null : await SubirImagenAsync(cfg, png, cancellationToken);
        }
        // Frontera de una función opcional: si el dibujo nativo (SkiaSharp) o la subida fallan por lo
        // que sea, se registra y se sigue con el texto. El aviso nunca se pierde por la tarjeta.
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "No se pudo preparar la tarjeta de WhatsApp '{Plantilla}': se envía el texto.", conTarjeta.Nombre);
            return null;
        }
    }

    private async Task<string?> SubirImagenAsync(ConfiguracionEnvio cfg, byte[] png, CancellationToken cancellationToken)
    {
        using var formulario = new MultipartFormDataContent();
        formulario.Add(new StringContent("whatsapp"), "messaging_product");
        formulario.Add(new StringContent("image/png"), "type");
        var archivo = new ByteArrayContent(png);
        archivo.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        formulario.Add(archivo, "file", "tarjeta.png");

        using var req = new HttpRequestMessage(HttpMethod.Post, cfg.Url("media"))
        {
            Content = formulario
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cfg.Token);
        using var resp = await _http.SendAsync(req, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            var cuerpo = await resp.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Subir la tarjeta a WhatsApp falló ({Status}): {Body}", (int)resp.StatusCode, cuerpo);
            return null;
        }
        var subida = await resp.Content.ReadFromJsonAsync<MediaSubida>(cancellationToken);
        return string.IsNullOrWhiteSpace(subida?.Id) ? null : subida.Id;
    }

    private static object[] ComponentesTexto(string[] parametros) => new object[]
    {
        new { type = "body", parameters = parametros.Select(p => new { type = "text", text = p }).ToArray() }
    };

    private static object[] ComponentesConTarjeta(string mediaId, string[] parametros) => new object[]
    {
        new { type = "header", parameters = new[] { new { type = "image", image = new { id = mediaId } } } },
        new { type = "body", parameters = parametros.Select(p => new { type = "text", text = p }).ToArray() }
    };

    /// <summary>Manda la plantilla a un destinatario. True si Meta la aceptó.</summary>
    private async Task<bool> EnviarAUnoAsync(ConfiguracionEnvio cfg, string destinatario, string plantilla,
        object[] componentes, CancellationToken cancellationToken)
    {
        try
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                to = destinatario,
                type = "template",
                template = new { name = plantilla, language = new { code = cfg.Idioma }, components = componentes }
            };
            using var req = new HttpRequestMessage(HttpMethod.Post, cfg.Url("messages"))
            {
                Content = JsonContent.Create(payload)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cfg.Token);
            using var resp = await _http.SendAsync(req, cancellationToken);
            if (resp.IsSuccessStatusCode)
                return true;
            var cuerpo = await resp.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("WhatsApp '{Plantilla}' a {To} falló ({Status}): {Body}", plantilla, destinatario, (int)resp.StatusCode, cuerpo);
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Error enviando WhatsApp '{Plantilla}' a {To}", plantilla, destinatario);
            return false;
        }
    }

    // ===== Formato =====

    private static string Litros(decimal valor) => valor.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>Con cultura fija: en un formato .NET la "/" es el separador de fecha de la cultura actual.</summary>
    private static string Dia(DateTime fecha) => fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    /// <summary>
    /// Meta rechaza el mensaje entero si una variable trae saltos de línea, tabulaciones o más de
    /// cuatro espacios seguidos. El texto libre se deja en una línea y se recorta por caracteres
    /// visibles, para no partir un emoji (par sustituto) por la mitad.
    /// </summary>
    private static string TextoDeVariable(string texto, int max)
    {
        var limpio = EspaciosSeguidos().Replace(texto, " ").Trim();
        var info = new StringInfo(limpio);
        return info.LengthInTextElements <= max ? limpio : info.SubstringByTextElements(0, max - 1) + "…";
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspaciosSeguidos();
}
