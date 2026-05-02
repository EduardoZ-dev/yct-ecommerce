namespace YCT.Application.DTOs;

public class DistributorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string VehicleType { get; set; } = "Moto";
    public string? VehiclePlate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrdersCount { get; set; }
}
