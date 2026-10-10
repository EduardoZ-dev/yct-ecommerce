using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YCT.Application.Common;

namespace YCT.Infrastructure.Services;

/// <summary>Un aviso esperando turno: qué es (para el log) y cómo ejecutarlo dentro de un scope.</summary>
public sealed record AvisoPendiente(string Descripcion, Func<IServiceProvider, CancellationToken, Task> Ejecutar);

/// <summary>
/// Cola en memoria de <see cref="IColaAvisos"/>. La vacía <see cref="AvisosEnSegundoPlanoService"/>.
/// Acotada: si algo se atasca, no crece sin límite; lo que no cabe se descarta con un aviso en el log.
/// </summary>
public sealed class ColaAvisos : IColaAvisos
{
    private const int Capacidad = 500;

    private readonly Channel<AvisoPendiente> _canal = Channel.CreateBounded<AvisoPendiente>(
        new BoundedChannelOptions(Capacidad) { SingleReader = true, FullMode = BoundedChannelFullMode.Wait });
    private readonly ILogger<ColaAvisos> _logger;

    public ColaAvisos(ILogger<ColaAvisos> logger) => _logger = logger;

    public ChannelReader<AvisoPendiente> Pendientes => _canal.Reader;

    public void Encolar<TServicio>(string descripcion, Func<TServicio, CancellationToken, Task> trabajo)
        where TServicio : notnull
    {
        ArgumentNullException.ThrowIfNull(trabajo);
        var aviso = new AvisoPendiente(descripcion,
            (servicios, ct) => trabajo(servicios.GetRequiredService<TServicio>(), ct));
        if (!_canal.Writer.TryWrite(aviso))
            _logger.LogWarning("Cola de avisos llena ({Capacidad}): se descarta '{Aviso}'", Capacidad, descripcion);
    }
}
