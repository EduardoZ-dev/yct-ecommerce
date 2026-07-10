namespace YCT.Domain.Common;

/// <summary>
/// Hora de Colombia. El servidor corre en otra zona (UTC) y guarda todo en UTC;
/// Colombia NO tiene horario de verano, siempre es UTC-5, así que basta un desfase fijo.
/// Usar esto para cualquier hora que se muestre o se registre "en hora local de Colombia".
/// </summary>
public static class ColombiaTime
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(-5);

    /// <summary>Ahora, en hora de Colombia.</summary>
    public static DateTime Now => DateTime.UtcNow + Offset;

    /// <summary>Hoy (fecha) en Colombia.</summary>
    public static DateTime Today => Now.Date;

    /// <summary>Convierte un instante guardado en UTC a hora de Colombia.</summary>
    public static DateTime FromUtc(DateTime utc) => utc + Offset;
}
