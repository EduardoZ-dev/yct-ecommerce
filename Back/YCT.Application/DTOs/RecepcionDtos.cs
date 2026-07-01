namespace YCT.Application.DTOs;

/// <summary>Respuesta del login de la tablet de recepción.</summary>
public class RecepcionLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// Planilla pendiente de descargar, vista por el receptor. VALIDACIÓN A CIEGAS:
/// NO incluye litros declarados por el chofer ni ninguna diferencia.
/// </summary>
public class RecepcionPendienteDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string CamionNombre { get; set; } = string.Empty;
    public string ConductorNombre { get; set; } = string.Empty;
    public int NumFincas { get; set; }
    /// <summary>Hora en que el chofer envió la planilla (para ordenar la fila de camiones).</summary>
    public DateTime EnviadoAt { get; set; }
}

/// <summary>Lo que el receptor envía: solo los litros que midió y una observación.</summary>
public class RecepcionValidarRequest
{
    public int PlanillaId { get; set; }
    public decimal LitrosPlanta { get; set; }
    public string? Observacion { get; set; }
}
