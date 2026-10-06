using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Cliente tercero: persona que NO es proveedor fijo de la ruta pero trae leche a la planta
/// de forma ocasional. No tiene número de cuaderno ni finca en ruta; se identifica por nombre/cédula.
/// Sus entregas entran al proceso total como cualquier otra leche.
/// </summary>
public class ClienteTercero : BaseEntity
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    public string? Municipio { get; set; }
    /// <summary>Precio por litro acordado con este cliente (COP). Se copia a cada entrega al registrarla.</summary>
    public decimal? PrecioLitro { get; set; }
    public string? Notas { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<EntregaTercero> Entregas { get; set; } = new List<EntregaTercero>();
}
