using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Recogida individual de leche a un granjero dentro de una ruta.
/// Doble entrada: lo que apuntó el chofer + lo medido en planta.
/// </summary>
public class Recogida : BaseEntity
{
    public int RutaId { get; set; }
    public Ruta Ruta { get; set; } = null!;

    public int GranjeroId { get; set; }
    public Granjero Granjero { get; set; } = null!;

    /// <summary>Código específico del granjero (finca/tanque) al que se le recogió. Null en planillas creadas a mano.</summary>
    public int? GranjeroCodigoId { get; set; }
    public GranjeroCodigo? GranjeroCodigo { get; set; }

    public DateTime Fecha { get; set; }

    // ===== Entrada 1: chofer en finca =====
    public int CantinasChofer { get; set; }
    public decimal SaldoChofer { get; set; }
    /// <summary>Calculado por el handler: CantinasChofer * 40 + SaldoChofer.</summary>
    public decimal LitrosChofer { get; set; }

    /// <summary>Litros que el granjero le regala al chofer (NO son de YCT, no cuentan en el total).</summary>
    public decimal LitrosRegaladosChofer { get; set; }

    // ===== Estado organoléptico con que el chofer recibe la leche =====
    public string? Observacion { get; set; }
    /// <summary>Vista/color: Normal | Mas clara | Mas oscura | Mal aspecto.</summary>
    public string? EstadoVista { get; set; }
    /// <summary>Olor: Normal | Sin olor | Olor fuerte.</summary>
    public string? EstadoOlor { get; set; }
    /// <summary>Sabor: Normal | Raro/malo.</summary>
    public string? EstadoSabor { get; set; }

    // ===== Recorrido: dónde y en qué orden se recogió =====
    /// <summary>Orden de visita dentro de la ruta (1, 2, 3...).</summary>
    public int Orden { get; set; }
    /// <summary>Momento en que el chofer capturó la recogida en la finca (hora local del dispositivo).</summary>
    public DateTime? CapturadoAt { get; set; }
    public double? GpsLat { get; set; }
    public double? GpsLng { get; set; }

    // ===== Entrada 2: planta al descargar (opcional, se llena después) =====
    public int? CantinasPlanta { get; set; }
    public decimal? SaldoPlanta { get; set; }
    public decimal? LitrosPlanta { get; set; }

    /// <summary>Diferencia LitrosPlanta - LitrosChofer (negativo = faltó leche).</summary>
    public decimal? DiferenciaLitros { get; set; }
    public string? MotivoDiferencia { get; set; }

    public DateTime RecogidoAt { get; set; } = DateTime.UtcNow;
    public DateTime? DescargadoAt { get; set; }

    public int? OperarioPlantaUserId { get; set; }
    public User? OperarioPlantaUser { get; set; }

    /// <summary>UUID generado en cliente para sincronización offline idempotente.</summary>
    public Guid? ClientUuid { get; set; }
    public DateTime? SyncedAt { get; set; }
}
