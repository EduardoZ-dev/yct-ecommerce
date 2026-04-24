namespace YCT.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // Info del producto
    public string? Weight { get; set; }
    public string? Ingredients { get; set; }
    public string? StorageInstructions { get; set; }
    public string? ExpirationInfo { get; set; }
    public string? Brand { get; set; }

    // Nutricional
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
}
