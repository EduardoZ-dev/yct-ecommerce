using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Products.GetAll;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, ResponseBase<List<ProductDto>>>
{
    private readonly IGenericRepository<Product> _repository;

    public GetAllProductsQueryHandler(IGenericRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<List<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.FindAsync(p => true, p => p.Category);

        var dtos = products
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Id)
            .Select(p => new ProductDto
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
                Brand = p.Brand,
                Weight = p.Weight,
                Ingredients = p.Ingredients,
                StorageInstructions = p.StorageInstructions,
                ExpirationInfo = p.ExpirationInfo,
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
            })
            .ToList();

        return ResponseBase<List<ProductDto>>.Ok(dtos);
    }
}
