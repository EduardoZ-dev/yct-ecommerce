using YCT.Domain.Common;

namespace YCT.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    /// <summary>Número correlativo simple (1, 2, 3...) para que el cliente lo recuerde.</summary>
    public int Consecutive { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public string Status { get; set; } = "Pending";

    /// <summary>OnDelivery (contra entrega), Transfer (transferencia previa), Cash (efectivo en sede)</summary>
    public string PaymentMethod { get; set; } = "OnDelivery";
    /// <summary>Unpaid, Paid, Refunded</summary>
    public string PaymentStatus { get; set; } = "Unpaid";
    public DateTime? PaidAt { get; set; }

    public string? Notes { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public decimal? ShippingLat { get; set; }
    public decimal? ShippingLng { get; set; }

    // Timestamps por etapa
    public DateTime? ValidatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Envío
    public int? DistributorId { get; set; }
    public Distributor? Distributor { get; set; }
    public string? TrackingNumber { get; set; }

    // Feedback final
    public int? CustomerRating { get; set; }
    public string? FeedbackComment { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
