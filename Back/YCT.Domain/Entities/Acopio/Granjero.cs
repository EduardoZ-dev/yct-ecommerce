using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Granjero proveedor de leche cruda. Identificado por número (corresponde al "No. del Proveedor" del cuaderno físico).
/// </summary>
public class Granjero : BaseEntity
{
    /// <summary>Número del proveedor (1, 4, 23, 71, etc). Único.</summary>
    public int Numero { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    public string? Finca { get; set; }
    public string? Vereda { get; set; }
    public string? Municipio { get; set; }
    /// <summary>Precio que se paga por cada litro a este granjero (COP). Usado para liquidaciones.</summary>
    public decimal? PrecioLitro { get; set; }
    /// <summary>Promedio diario de litros entregados. Calculado por job periódico.</summary>
    public decimal? PromedioDiario { get; set; }
    public string? Notas { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Códigos/sub-números del proveedor (uno por finca/tanque). La recogida se hace por código.</summary>
    public ICollection<GranjeroCodigo> Codigos { get; set; } = new List<GranjeroCodigo>();

    public ICollection<Recogida> Recogidas { get; set; } = new List<Recogida>();
}
