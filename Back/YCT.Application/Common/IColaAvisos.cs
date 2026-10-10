namespace YCT.Application.Common;

/// <summary>
/// Avisos (WhatsApp, correo…) que se mandan DESPUÉS de responder: un aviso lento o caído nunca
/// hace esperar ni falla una operación del negocio. Cada trabajo corre en su propio scope de DI.
/// </summary>
public interface IColaAvisos
{
    /// <summary>
    /// Encola un trabajo que usa <typeparamref name="TServicio"/> (se resuelve del contenedor al
    /// ejecutarse). Nunca lanza: si la cola está llena, el aviso se descarta y queda en el log.
    /// </summary>
    void Encolar<TServicio>(string descripcion, Func<TServicio, CancellationToken, Task> trabajo)
        where TServicio : notnull;
}
