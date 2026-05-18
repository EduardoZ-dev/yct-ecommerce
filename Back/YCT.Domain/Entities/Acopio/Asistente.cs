using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Asistente del camión. Sube y baja tinajas. No usa la app.
/// Se registra solo para información de nómina e historial.
/// </summary>
public class Asistente : BaseEntity
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    /// <summary>Camión donde normalmente trabaja.</summary>
    public int? CamionPreferidoId { get; set; }
    public Camion? CamionPreferido { get; set; }
    public bool IsActive { get; set; } = true;
}
