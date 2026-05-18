using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Ruta diaria de recolección. Asocia un camión, un conductor y un asistente para una fecha específica.
/// Contiene N recogidas (una por granjero visitado).
/// </summary>
public class Ruta : BaseEntity
{
    /// <summary>Código de la ruta (ej. nombre del camión: DONFENG, NISSAN, HUGO, JMC).</summary>
    public string Codigo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }

    public int CamionId { get; set; }
    public Camion Camion { get; set; } = null!;

    public int ConductorId { get; set; }
    public Conductor Conductor { get; set; } = null!;

    public int? AsistenteId { get; set; }
    public Asistente? Asistente { get; set; }

    public TimeSpan? HoraSalida { get; set; }
    public TimeSpan? HoraLlegadaPlanta { get; set; }
    public TimeSpan? HoraDescargue { get; set; }

    /// <summary>Total litros que el chofer registró durante la ruta (suma de recogidas).</summary>
    public decimal TotalLitrosChofer { get; set; }
    /// <summary>Total litros realmente descargados en planta (validación).</summary>
    public decimal? TotalLitrosPlanta { get; set; }
    /// <summary>Diferencia entre lo planta y lo chofer (negativo = faltó).</summary>
    public decimal? DiferenciaTotal { get; set; }

    /// <summary>EnProgreso, EsperandoDescargue, Conciliada, Anulada.</summary>
    public string Status { get; set; } = "EnProgreso";
    public string? Observaciones { get; set; }

    public ICollection<Recogida> Recogidas { get; set; } = new List<Recogida>();
}
