using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Conductor que opera un camión recolector de leche.
/// Puede tener cuenta de usuario asociada para login en la app móvil.
/// </summary>
public class Conductor : BaseEntity
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    /// <summary>Camión que normalmente conduce (referencia, no obligatoria por día).</summary>
    public int? CamionPreferidoId { get; set; }
    public Camion? CamionPreferido { get; set; }
    /// <summary>Usuario shared.Users si tiene login en app móvil.</summary>
    public int? UserId { get; set; }
    public User? User { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Ruta> Rutas { get; set; } = new List<Ruta>();
}
