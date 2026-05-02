namespace YCT.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int Consecutive { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "OnDelivery";
    public string PaymentStatus { get; set; } = "Unpaid";
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public decimal? ShippingLat { get; set; }
    public decimal? ShippingLng { get; set; }

    // Etapas
    public DateTime? ValidatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Envío
    public int? DistributorId { get; set; }
    public string? DistributorName { get; set; }
    public string? DistributorVehicle { get; set; }
    public string? DistributorPhone { get; set; }
    public string? TrackingNumber { get; set; }

    // Feedback
    public int? CustomerRating { get; set; }
    public string? FeedbackComment { get; set; }

    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? UserPhone { get; set; }
    public List<OrderDetailDto> Details { get; set; } = new();
}

public class OrderDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>DTO público para tracking — sin info sensible</summary>
public class TrackedOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public int Consecutive { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "OnDelivery";
    public string PaymentStatus { get; set; } = "Unpaid";
    public DateTime? ValidatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? DistributorName { get; set; }
    public string? DistributorVehicle { get; set; }
    public string? TrackingNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? ShippingCity { get; set; }
    public decimal? ShippingLat { get; set; }
    public decimal? ShippingLng { get; set; }
    public List<OrderDetailDto> Items { get; set; } = new();
}
