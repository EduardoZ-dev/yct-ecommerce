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

    public DateTime Fecha { get; set; }

    // ===== Entrada 1: chofer en finca =====
    public int CantinasChofer { get; set; }
    public decimal SaldoChofer { get; set; }
    /// <summary>Calculado por el handler: CantinasChofer * 40 + SaldoChofer.</summary>
    public decimal LitrosChofer { get; set; }

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
