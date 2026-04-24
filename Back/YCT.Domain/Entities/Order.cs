using YCT.Domain.Common;

namespace YCT.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public string? ShippingAddress { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
