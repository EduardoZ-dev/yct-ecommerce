using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Products.GetCatalog;

public class GetCatalogQueryHandler : IRequestHandler<GetCatalogQuery, ResponseBase<List<ProductDto>>>
{
    private readonly IGenericRepository<Product> _repository;

    public GetCatalogQueryHandler(IGenericRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<List<ProductDto>>> Handle(GetCatalogQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.FindAsync(p => p.IsActive && p.Stock > 0, p => p.Category);
        var dtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl,
            IsActive = p.IsActive,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            Weight = p.Weight,
            Ingredients = p.Ingredients,
            StorageInstructions = p.StorageInstructions,
            ExpirationInfo = p.ExpirationInfo,
            Brand = p.Brand,
            ServingSize = p.ServingSize,
            Calories = p.Calories,
            TotalFat = p.TotalFat,
            SaturatedFat = p.SaturatedFat,
            Cholesterol = p.Cholesterol,
            Sodium = p.Sodium,
            TotalCarbs = p.TotalCarbs,
            Sugars = p.Sugars,
            Protein = p.Protein,
            Calcium = p.Calcium,
            Iron = p.Iron,
            VitaminD = p.VitaminD
        }).ToList();

        return ResponseBase<List<ProductDto>>.Ok(dtos);
    }
}
