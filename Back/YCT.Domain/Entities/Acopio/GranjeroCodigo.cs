using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Código interno (sub-número) de un granjero. Un mismo proveedor (NIT/cédula) puede tener
/// varios códigos —uno por finca/tanque— como en el cuaderno físico. La recogida se hace por código.
/// </summary>
public class GranjeroCodigo : BaseEntity
{
    public int GranjeroId { get; set; }
    public Granjero Granjero { get; set; } = null!;

    /// <summary>Código tal cual el cuaderno (ej. "008", "071", "005"). No necesariamente único global.</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Finca/predio al que corresponde este código.</summary>
    public string? Finca { get; set; }

    /// <summary>
    /// Cantidad de tinas (cantinas) propiedad de YCT en esta finca. 0 = no tenemos ninguna
    /// tina nuestra ahí; un número = esas tinas son de YCT. Dato tomado del Excel de rutas.
    /// </summary>
    public int TinasYct { get; set; }

    public bool IsActive { get; set; } = true;
}
