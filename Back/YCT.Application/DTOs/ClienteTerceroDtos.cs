namespace YCT.Application.DTOs;

// ===== Cliente tercero =====
public class ClienteTerceroDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    public string? Municipio { get; set; }
    public decimal? PrecioLitro { get; set; }
    public string? Notas { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Resumen histórico de sus entregas. Se calcula en la consulta, no se persiste.</summary>
    public int TotalEntregas { get; set; }
    public decimal TotalLitros { get; set; }
    public DateTime? UltimaEntrega { get; set; }
}

// ===== Entrega de tercero =====
public class EntregaTerceroDto
{
    public int Id { get; set; }
    public int ClienteTerceroId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int Cantinas { get; set; }
    public decimal SaldoLitros { get; set; }
    public decimal Litros { get; set; }
    public decimal? PrecioLitro { get; set; }
    /// <summary>Litros × precio por litro. Null cuando la entrega no tiene precio.</summary>
    public decimal? Valor { get; set; }
    public string? Observacion { get; set; }
    /// <summary>"Panel" o "Tablet" (ver constantes de la entidad EntregaTercero).</summary>
    public string Origen { get; set; } = string.Empty;
    public int? RutaId { get; set; }
    /// <summary>Código de la ruta del descargue en que llegó la leche; null si se registró a mano.</summary>
    public string? RutaCodigo { get; set; }
    public string? RegistradoPorNombre { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>True cuando el receptor en planta ya reafirmó que la leche llegó.</summary>
    public bool Confirmada { get; set; }
    public DateTime? ConfirmadaEnPlantaAt { get; set; }
    public string? ConfirmadaPorNombre { get; set; }

    /// <summary>Lo que midió el receptor en planta. Null si la llegada se marcó sin medir.</summary>
    public int? CantinasPlanta { get; set; }
    public decimal? SaldoPlanta { get; set; }
    public decimal? LitrosPlanta { get; set; }
    /// <summary>Recibido − registrado: negativo = llegó menos leche. Null si no se midió.</summary>
    public decimal? DiferenciaLitros { get; set; }
    public string? ObservacionPlanta { get; set; }
}

/// <summary>
/// Referencia de a cuánto se ha venido comprando la leche a terceros. El promedio es
/// PONDERADO por litros (plata pagada ÷ litros comprados), que es el precio real al que
/// se compró; un promedio simple de precios engañaría si una entrega grande fue más cara.
/// </summary>
public class PromedioCompraTerceroDto
{
    /// <summary>Null cuando todavía no hay ninguna entrega con precio.</summary>
    public decimal? PrecioPromedio { get; set; }
    public decimal? PrecioMinimo { get; set; }
    public decimal? PrecioMaximo { get; set; }
    /// <summary>Entregas CON precio que entraron en el cálculo.</summary>
    public int Entregas { get; set; }
    public decimal Litros { get; set; }
    public decimal ValorTotal { get; set; }
    /// <summary>Fecha de la entrega más antigua considerada.</summary>
    public DateTime? Desde { get; set; }
}
