namespace YCT.Application.DTOs;

// ===== Camion =====
public class CamionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Placa { get; set; }
    public string Estado { get; set; } = "Activo";
    public string? Notas { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ===== Conductor =====
public class ConductorDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    public int? CamionPreferidoId { get; set; }
    public string? CamionPreferidoNombre { get; set; }
    public int? UserId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ===== Asistente =====
public class AsistenteDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    public int? CamionPreferidoId { get; set; }
    public string? CamionPreferidoNombre { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ===== Granjero =====
public class GranjeroDto
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    public string? Finca { get; set; }
    public string? Vereda { get; set; }
    public string? Municipio { get; set; }
    public decimal? PrecioLitro { get; set; }
    public decimal? PromedioDiario { get; set; }
    public string? Notas { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
