using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Camión recolector de leche. Identificado por nombre (Donfeng, Nissan, Hugo, JMC).
/// </summary>
public class Camion : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Placa { get; set; }
    /// <summary>Activo, Mantenimiento, Inactivo</summary>
    public string Estado { get; set; } = "Activo";
    public string? Notas { get; set; }

    public ICollection<Ruta> Rutas { get; set; } = new List<Ruta>();
}
