namespace YCT.Application.DTOs;

public class ChoferEnvioRequest
{
    public string Uuid { get; set; } = string.Empty;        // ClientUuid planilla
    public DateTime Fecha { get; set; }
    public int CamionId { get; set; }
    public int ConductorId { get; set; }
    public int? AsistenteId { get; set; }
    public TimeSpan? HoraSalida { get; set; }
    public List<ChoferRecogidaItem> Recogidas { get; set; } = new();
}

/// <summary>
/// Novedad reportada por el chofer durante la ruta (llanta averiada, trancón, finca sin
/// ordeño…). Se envía apenas ocurre, antes de que exista la Ruta en el servidor: por eso
/// se ata por PlanillaUuid.
/// </summary>
public class ChoferNovedadRequest
{
    public string Uuid { get; set; } = string.Empty;         // idempotencia
    public string PlanillaUuid { get; set; } = string.Empty; // ruta a la que pertenece
    public int CamionId { get; set; }
    public string Categoria { get; set; } = string.Empty;    // Camion | Via | Finca | Otro
    public string Tipo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? GranjeroCodigoId { get; set; }               // solo novedades de finca
    public string ReportadoAt { get; set; } = string.Empty;  // ISO, hora de la tablet
    public decimal? GpsLat { get; set; }
    public decimal? GpsLng { get; set; }
}

public class ChoferRecogidaItem
{
    public string Uuid { get; set; } = string.Empty;
    public int GranjeroId { get; set; }
    public int? GranjeroCodigoId { get; set; }
    public int Cantinas { get; set; }
    public decimal SaldoLitros { get; set; }
    public decimal LitrosRegaladosChofer { get; set; }
    public string? Observacion { get; set; }
    public string? EstadoVista { get; set; }
    public string? EstadoOlor { get; set; }
    public string? EstadoSabor { get; set; }
    public string CapturadoAt { get; set; } = string.Empty;
    public double? GpsLat { get; set; }
    public double? GpsLng { get; set; }
}
