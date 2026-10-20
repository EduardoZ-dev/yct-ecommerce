using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YCT.Application.Common;

namespace YCT.Infrastructure.Services.Tarjetas;

/// <summary>
/// Al arrancar dibuja una tarjeta de prueba y deja en el log si el dibujo funciona en este servidor
/// ("Tarjetas WhatsApp: OK"). Así se comprueba la librería nativa en Linux sin esperar a un descargue
/// real. Nunca detiene el arranque: si falla, los avisos salen en texto como siempre.
/// </summary>
public sealed class VerificacionTarjetasWhatsApp : IHostedService
{
    private readonly ITarjetasWhatsApp _tarjetas;
    private readonly ILogger<VerificacionTarjetasWhatsApp> _logger;

    public VerificacionTarjetasWhatsApp(ITarjetasWhatsApp tarjetas, ILogger<VerificacionTarjetasWhatsApp> logger)
    {
        _tarjetas = tarjetas;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var descargue = _tarjetas.Descargue(new WhatsAppDescargueModel("✅ OK", "PRUEBA-0000", DateTime.Today,
                "Conductor de prueba", "CAMIÓN", 100, 100, 0, "Conciliada", "", "",
                new[] { new AporteFinca("Finca de prueba", 100) }));
            var tercero = _tarjetas.Tercero(new WhatsAppTerceroModel("✅ Llegó completa", "Cliente de prueba",
                DateTime.Today, 40, 40, 0, "", "Planta"));
            _logger.LogInformation("Tarjetas WhatsApp: OK (descargue {Descargue} bytes, tercero {Tercero} bytes)",
                descargue.Length, tercero.Length);
        }
        // Frontera de arranque: una falla de la librería nativa no puede tumbar la API.
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tarjetas WhatsApp: FALLÓ el dibujo; los avisos saldrán en texto");
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
