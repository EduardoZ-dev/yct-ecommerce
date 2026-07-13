namespace YCT.Application.DTOs;

/// <summary>
/// Planilla = una salida diaria de un camión recolectando leche.
/// Internamente se mapea a la entidad Ruta + sus Recogidas.
/// </summary>
public class PlanillaHeaderDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;          // ej: "DONFENG"
    public DateTime Fecha { get; set; }
    public int CamionId { get; set; }
    public string CamionNombre { get; set; } = string.Empty;
    public int ConductorId { get; set; }
    public string ConductorNombre { get; set; } = string.Empty;
    public int? AsistenteId { get; set; }
    public string? AsistenteNombre { get; set; }
    public TimeSpan? HoraSalida { get; set; }
    public TimeSpan? HoraLlegadaPlanta { get; set; }
    public TimeSpan? HoraDescargue { get; set; }
    public decimal TotalLitros { get; set; }
    public decimal? TotalLitrosPlanta { get; set; }
    public decimal? DiferenciaTotal { get; set; }
    public int TotalCantinas { get; set; }
    public int TotalRecogidas { get; set; }
    public string Status { get; set; } = "EnProgreso";
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PlanillaDto : PlanillaHeaderDto
{
    public List<PlanillaItemDto> Items { get; set; } = new();

    /// <summary>Imprevistos que el chofer reportó durante la ruta (para tenerlos en cuenta al validar).</summary>
    public List<PlanillaNovedadDto> Novedades { get; set; } = new();
}

/// <summary>Novedad reportada por el chofer en ruta (llanta averiada, trancón, finca sin ordeño…).</summary>
public class PlanillaNovedadDto
{
    public string Categoria { get; set; } = string.Empty;   // Camion | Via | Finca | Otro
    public string Tipo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Finca { get; set; }                      // solo novedades de finca
    public DateTime ReportadoAt { get; set; }
}

public class PlanillaItemDto
{
    public long? Id { get; set; }
    public int GranjeroId { get; set; }
    public int GranjeroNumero { get; set; }
    public string GranjeroNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int Cantinas { get; set; }
    public decimal SaldoLitros { get; set; }
    public decimal TotalLitros { get; set; }
    // Estado, sobrante y recorrido
    public decimal LitrosRegaladosChofer { get; set; }
    public string? Observacion { get; set; }
    public string? EstadoVista { get; set; }
    public string? EstadoOlor { get; set; }
    public string? EstadoSabor { get; set; }
    public int Orden { get; set; }
    public DateTime? CapturadoAt { get; set; }
    public double? GpsLat { get; set; }
    public double? GpsLng { get; set; }
}

public class SavePlanillaItemRequest
{
    public long? Id { get; set; }
    public int GranjeroId { get; set; }
    public int? GranjeroCodigoId { get; set; }
    public DateTime Fecha { get; set; }
    public int Cantinas { get; set; }
    public decimal SaldoLitros { get; set; }
    public decimal LitrosRegaladosChofer { get; set; }
    public string? Observacion { get; set; }
    public string? EstadoVista { get; set; }
    public string? EstadoOlor { get; set; }
    public string? EstadoSabor { get; set; }
    public int Orden { get; set; }
    public DateTime? CapturadoAt { get; set; }
    public double? GpsLat { get; set; }
    public double? GpsLng { get; set; }
}

public class SendPlanillaEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? PdfBase64 { get; set; }
    public string? PdfFileName { get; set; }
}

/// <summary>Operario planta valida total litros descargados.</summary>
public class ValidatePlantaRequest
{
    public decimal TotalLitrosPlanta { get; set; }
    public TimeSpan? HoraDescargue { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>Admin autoriza/anula planilla con faltante.</summary>
public class AuthorizeShortageRequest
{
    public bool Approve { get; set; }
    public string? Motivo { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? PlanillaId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Novedad de ruta para el panel. Llega ANTES que el descargue: explica por qué un
/// camión viene retrasado o por qué va a faltar leche, sin esperar a que termine la ruta.
/// </summary>
public class NovedadDto
{
    public int Id { get; set; }
    public string Categoria { get; set; } = string.Empty;   // Camion | Via | Finca | Otro
    public string Tipo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Finca { get; set; }
    public string? CodigoFinca { get; set; }
    public string ConductorNombre { get; set; } = string.Empty;
    public string CamionNombre { get; set; } = string.Empty;
    public DateTime ReportadoAt { get; set; }
    public decimal? GpsLat { get; set; }
    public decimal? GpsLng { get; set; }
    /// <summary>Ruta ligada. Null si el chofer aún no ha enviado la planilla (lo normal al reportar).</summary>
    public int? RutaId { get; set; }
    public string? RutaCodigo { get; set; }
    public string? RutaStatus { get; set; }
    public bool Revisada { get; set; }
    public DateTime? RevisadaAt { get; set; }
    public string? RevisadaPor { get; set; }
}
