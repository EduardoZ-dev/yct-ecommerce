using YCT.Domain.Common;

namespace YCT.Domain.Entities;

/// <summary>
/// Distribuidor / transportista que entrega los pedidos al cliente final.
/// Puede ser un domiciliario propio o una transportadora.
/// </summary>
public class Distributor : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    /// <summary>Moto, Camioneta, Camion, Externo (transportadora)</summary>
    public string VehicleType { get; set; } = "Moto";
    public string? VehiclePlate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
