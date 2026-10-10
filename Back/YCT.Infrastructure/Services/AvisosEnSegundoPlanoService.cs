using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YCT.Infrastructure.Services;

/// <summary>
/// Ejecuta, uno a uno y fuera de la petición, los avisos encolados en <see cref="ColaAvisos"/>.
/// Cada aviso tiene su propio scope (repositorios, DbContext, HttpClient frescos).
/// </summary>
public sealed class AvisosEnSegundoPlanoService : BackgroundService
{
    private readonly ColaAvisos _cola;
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<AvisosEnSegundoPlanoService> _logger;

    public AvisosEnSegundoPlanoService(
        ColaAvisos cola, IServiceScopeFactory scopes, ILogger<AvisosEnSegundoPlanoService> logger)
    {
        _cola = cola;
        _scopes = scopes;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var aviso in _cola.Pendientes.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopes.CreateAsyncScope();
                await aviso.Ejecutar(scope.ServiceProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            // Frontera del proceso: un aviso que falla se registra y se sigue con el siguiente. Si la
            // excepción escapara de ExecuteAsync, .NET detendría la API entera.
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falló el aviso en segundo plano: {Aviso}", aviso.Descripcion);
            }
        }
    }
}
