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
    public int TotalCantinas { get; set; }
    public int TotalRecogidas { get; set; }
    public string Status { get; set; } = "EnProgreso";
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PlanillaDto : PlanillaHeaderDto
{
    public List<PlanillaItemDto> Items { get; set; } = new();
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
}

public class SavePlanillaItemRequest
{
    public long? Id { get; set; }
    public int GranjeroId { get; set; }
    public DateTime Fecha { get; set; }
    public int Cantinas { get; set; }
    public decimal SaldoLitros { get; set; }
}

public class SendPlanillaEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Body { get; set; }
}
