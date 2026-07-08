using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Registro histórico de un ajuste de tinas (cantinas) propiedad de YCT, ya sea en una
/// finca (código de granjero) o en la propia planta. Deja trazabilidad de quién cambió
/// la cantidad, de cuánto a cuánto, cuándo y por qué (observación).
/// </summary>
public class TinaMovimiento : BaseEntity
{
    /// <summary>Código/finca afectada. Null cuando el ajuste es en la planta de YCT.</summary>
    public int? GranjeroCodigoId { get; set; }
    public GranjeroCodigo? GranjeroCodigo { get; set; }

    /// <summary>True si el ajuste corresponde a las tinas de la planta de YCT.</summary>
    public bool EsPlanta { get; set; }

    public int CantidadAnterior { get; set; }
    public int CantidadNueva { get; set; }

    /// <summary>Motivo/observación del ajuste (ej. "se llevaron 2 nuevas", "tina dañada").</summary>
    public string? Observacion { get; set; }

    /// <summary>Nombre del usuario del panel que hizo el ajuste (para auditoría legible).</summary>
    public string? UsuarioNombre { get; set; }
}
