namespace YCT.Application.DTOs;

// ===== Administración de tinas (cantinas propiedad de YCT) =====

/// <summary>Tinas de YCT en una finca (código de granjero).</summary>
public class TinaFincaDto
{
    public int CodigoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? Finca { get; set; }
    public int GranjeroId { get; set; }
    public int GranjeroNumero { get; set; }
    public string GranjeroNombre { get; set; } = string.Empty;
    public int TinasYct { get; set; }
}

/// <summary>Tinas de YCT que están en la planta.</summary>
public class TinaPlantaDto
{
    public int Cantidad { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>Un ajuste histórico de tinas (finca o planta).</summary>
public class TinaMovimientoDto
{
    public int Id { get; set; }
    public bool EsPlanta { get; set; }
    public int? CodigoId { get; set; }
    public string? Codigo { get; set; }
    public string? Finca { get; set; }
    public string? Granjero { get; set; }
    public int CantidadAnterior { get; set; }
    public int CantidadNueva { get; set; }
    public string? Observacion { get; set; }
    public string? Usuario { get; set; }
    public DateTime Fecha { get; set; }
}

/// <summary>Todo lo que necesita la pantalla de administración de tinas.</summary>
public class TinasOverviewDto
{
    public List<TinaFincaDto> Fincas { get; set; } = new();
    public TinaPlantaDto Planta { get; set; } = new();
    public List<TinaMovimientoDto> Historial { get; set; } = new();
    public int TotalTinasFincas { get; set; }
}
