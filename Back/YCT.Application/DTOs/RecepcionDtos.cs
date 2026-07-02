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
    /// <summary>
    /// Novedades de calidad que registró el chofer por finca (para el seguimiento en planta).
    /// NO incluye litros, así que no compromete la validación a ciegas.
    /// </summary>
    public List<RecepcionNovedadDto> NovedadesLeche { get; set; } = new();
}

/// <summary>Novedad de la leche que el chofer registró en una finca (sin litros).</summary>
public class RecepcionNovedadDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Granjero { get; set; } = string.Empty;
    public string? Finca { get; set; }
    /// <summary>Estados no-normales (ej. "Olor fuerte · Mal aspecto"); vacío si todo normal.</summary>
    public string? Estado { get; set; }
    public string? Observacion { get; set; }
}

/// <summary>
/// Lo que el receptor envía: el total medido + observación, con el desglose de planta
/// (tinas × 40 + sueltos) para dejar la misma proyección que el chofer.
/// </summary>
public class RecepcionValidarRequest
{
    public int PlanillaId { get; set; }
    public decimal LitrosPlanta { get; set; }
    public int? CantinasPlanta { get; set; }
    public decimal? LitrosSueltosPlanta { get; set; }
    public string? Observacion { get; set; }
}
