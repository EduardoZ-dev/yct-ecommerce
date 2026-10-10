using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Entrega ocasional de leche de un cliente tercero. Se registra desde el panel de acopio y,
/// más adelante, desde la tablet en el momento del descargue (por eso guarda la ruta y un UUID
/// de cliente para sincronización idempotente, igual que <see cref="Recogida"/>).
/// </summary>
public class EntregaTercero : BaseEntity
{
    /// <summary>Litros que caben en una cantina estándar. Mismo factor que usan las recogidas de ruta.</summary>
    public const decimal LitrosPorCantina = 40m;

    public const string OrigenPanel = "Panel";
    public const string OrigenTablet = "Tablet";

    public int ClienteTerceroId { get; set; }
    public ClienteTercero ClienteTercero { get; set; } = null!;

    /// <summary>Día de la entrega (sin hora).</summary>
    public DateTime Fecha { get; set; }

    public int Cantinas { get; set; }
    /// <summary>Litros sueltos que no completan una cantina.</summary>
    public decimal SaldoLitros { get; set; }
    /// <summary>Calculado por el handler: Cantinas * <see cref="LitrosPorCantina"/> + SaldoLitros.</summary>
    public decimal Litros { get; set; }

    /// <summary>Precio por litro al momento de la entrega (copiado del cliente, editable por entrega).</summary>
    public decimal? PrecioLitro { get; set; }
    public string? Observacion { get; set; }

    /// <summary>"Panel" si se registró en acopio; "Tablet" si la capturó el chofer en el descargue.</summary>
    public string Origen { get; set; } = OrigenPanel;

    /// <summary>Ruta/planilla del descargue en que llegó esta leche. Null cuando se registra a mano en el panel.</summary>
    public int? RutaId { get; set; }

    public int? RegistradoPorUserId { get; set; }
    public string? RegistradoPorNombre { get; set; }

    /// <summary>UUID generado en cliente para sincronización offline idempotente (tablet).</summary>
    public Guid? ClientUuid { get; set; }

    /// <summary>Cuándo el receptor en planta confirmó que esta leche llegó. Null = aún por confirmar.</summary>
    public DateTime? ConfirmadaEnPlantaAt { get; set; }
    public int? ConfirmadaPorUserId { get; set; }
    public string? ConfirmadaPorNombre { get; set; }

    // ===== Medición en planta (la hace el receptor al confirmar, A CIEGAS) =====
    // La leche viaja y puede pasar algo en el camino (derrame, cantina rota…): el receptor mide lo
    // que de verdad llegó sin ver lo registrado. Null = la llegada se marcó sin medir (desde el
    // panel, o antes de existir la medición). Lo registrado (Cantinas/SaldoLitros/Litros) NO se
    // toca: el pago lo maneja planta; esto es el control de la variación.
    public int? CantinasPlanta { get; set; }
    public decimal? SaldoPlanta { get; set; }
    public decimal? LitrosPlanta { get; set; }
    /// <summary>Lo que el receptor anotó al recibir (opcional).</summary>
    public string? ObservacionPlanta { get; set; }
}
