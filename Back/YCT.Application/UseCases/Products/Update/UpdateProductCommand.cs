using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Products.Update;

public record UpdateProductCommand(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string? ImageUrl,
    bool IsActive,
    int CategoryId,
    string? Brand = null,
    string? Weight = null,
    string? Ingredients = null,
    string? StorageInstructions = null,
    string? ExpirationInfo = null,
    string? ServingSize = null,
    decimal? Calories = null,
    decimal? TotalFat = null,
    decimal? SaturatedFat = null,
    decimal? Cholesterol = null,
    decimal? Sodium = null,
    decimal? TotalCarbs = null,
    decimal? Sugars = null,
    decimal? Protein = null,
    decimal? Calcium = null,
    decimal? Iron = null,
    decimal? VitaminD = null
) : IRequest<ResponseBase<ProductDto>>;
