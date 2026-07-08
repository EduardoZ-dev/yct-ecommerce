using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Cantidad de tinas (cantinas) propiedad de YCT que están en la planta (no en fincas).
/// Es una fila única (Id = 1); se ajusta a mano y su seguimiento queda en <see cref="TinaMovimiento"/>.
/// </summary>
public class TinaPlanta : BaseEntity
{
    public int Cantidad { get; set; }
}
