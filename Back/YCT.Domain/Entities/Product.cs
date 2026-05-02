using YCT.Domain.Common;

namespace YCT.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Info del producto
    public string? Weight { get; set; }
    public string? Ingredients { get; set; }
    public string? StorageInstructions { get; set; }
    public string? ExpirationInfo { get; set; }
    public string? Brand { get; set; }

    // Info nutricional (por porción)
    public string? ServingSize { get; set; }
    public decimal? Calories { get; set; }
    public decimal? TotalFat { get; set; }
    public decimal? SaturatedFat { get; set; }
    public decimal? Cholesterol { get; set; }
    public decimal? Sodium { get; set; }
    public decimal? TotalCarbs { get; set; }
    public decimal? Sugars { get; set; }
    public decimal? Protein { get; set; }
    public decimal? Calcium { get; set; }
    public decimal? Iron { get; set; }
    public decimal? VitaminD { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
