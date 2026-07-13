using YCT.Domain.Common;

namespace YCT.Domain.Entities.Acopio;

/// <summary>
/// Novedad/imprevisto reportado por el chofer DURANTE la ruta (llanta averiada, trancón,
/// finca sin ordeño, etc.). Se envía apenas ocurre —no al final— para que en la sede
/// puedan reaccionar y para tenerla en cuenta al validar el descargue.
///
/// Llega ANTES de que exista la Ruta en el servidor (el chofer aún no ha enviado la
/// planilla), por eso se ata por <see cref="PlanillaUuid"/> —el mismo UUID que la app usa
/// para la planilla— y el <see cref="RutaId"/> se rellena cuando la ruta finalmente llega.
/// </summary>
public class RutaNovedad : BaseEntity
{
    /// <summary>UUID de la novedad generado en la tablet. Idempotencia: no se duplica al reenviar.</summary>
    public string Uuid { get; set; } = string.Empty;

    /// <summary>UUID de la planilla/ruta a la que pertenece (Ruta.ChoferUuid).</summary>
    public string PlanillaUuid { get; set; } = string.Empty;

    /// <summary>Ruta ya creada en el servidor. Null mientras el chofer no haya enviado la planilla.</summary>
    public int? RutaId { get; set; }
    public Ruta? Ruta { get; set; }

    public int ConductorId { get; set; }
    public Conductor? Conductor { get; set; }

    public int CamionId { get; set; }
    public Camion? Camion { get; set; }

    /// <summary>Camion / Via / Finca / Otro.</summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Tipo puntual (ej. "Llanta averiada", "Trancón", "Finca sin ordeño").</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Detalle escrito por el chofer. Obligatorio cuando el tipo es "Otro".</summary>
    public string? Descripcion { get; set; }

    /// <summary>Finca afectada. Solo aplica a las novedades de categoría "Finca".</summary>
    public int? GranjeroCodigoId { get; set; }
    public GranjeroCodigo? GranjeroCodigo { get; set; }

    /// <summary>Momento en que ocurrió, en hora de Colombia (lo marca la tablet).</summary>
    public DateTime ReportadoAt { get; set; }

    public decimal? GpsLat { get; set; }
    public decimal? GpsLng { get; set; }
}
