using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Products.GetById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ResponseBase<ProductDto>>
{
    private readonly IGenericRepository<Product> _repository;

    public GetProductByIdQueryHandler(IGenericRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.FindAsync(p => p.Id == request.Id, p => p.Category);
        var product = products.FirstOrDefault();
        if (product == null)
            return ResponseBase<ProductDto>.Fail("Producto no encontrado");

        return ResponseBase<ProductDto>.Ok(new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            Weight = product.Weight,
            Ingredients = product.Ingredients,
            StorageInstructions = product.StorageInstructions,
            ExpirationInfo = product.ExpirationInfo,
            Brand = product.Brand,
            ServingSize = product.ServingSize,
            Calories = product.Calories,
            TotalFat = product.TotalFat,
            SaturatedFat = product.SaturatedFat,
            Cholesterol = product.Cholesterol,
            Sodium = product.Sodium,
            TotalCarbs = product.TotalCarbs,
            Sugars = product.Sugars,
            Protein = product.Protein,
            Calcium = product.Calcium,
            Iron = product.Iron,
            VitaminD = product.VitaminD
        });
    }
}
