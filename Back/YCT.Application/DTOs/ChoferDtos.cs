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
