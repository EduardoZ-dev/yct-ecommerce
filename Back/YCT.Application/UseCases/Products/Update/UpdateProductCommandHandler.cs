using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Products.Update;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ResponseBase<ProductDto>>
{
    private readonly IGenericRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public UpdateProductCommandHandler(IGenericRepository<Product> repository, IUnitOfWork unitOfWork, IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id);
        if (product == null)
            return ResponseBase<ProductDto>.Fail("Producto no encontrado");

        var before = new
        {
            product.Name, product.Price, product.Stock,
            product.IsActive, product.CategoryId
        };

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.ImageUrl = request.ImageUrl;
        product.IsActive = request.IsActive;
        product.CategoryId = request.CategoryId;
        product.Brand = request.Brand;
        product.Weight = request.Weight;
        product.Ingredients = request.Ingredients;
        product.StorageInstructions = request.StorageInstructions;
        product.ExpirationInfo = request.ExpirationInfo;
        product.ServingSize = request.ServingSize;
        product.Calories = request.Calories;
        product.TotalFat = request.TotalFat;
        product.SaturatedFat = request.SaturatedFat;
        product.Cholesterol = request.Cholesterol;
        product.Sodium = request.Sodium;
        product.TotalCarbs = request.TotalCarbs;
        product.Sugars = request.Sugars;
        product.Protein = request.Protein;
        product.Calcium = request.Calcium;
        product.Iron = request.Iron;
        product.VitaminD = request.VitaminD;
        product.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Update", "Product", product.Id,
            $"Producto actualizado: {product.Name}",
            new
            {
                before,
                after = new { product.Name, product.Price, product.Stock, product.IsActive, product.CategoryId }
            },
            ct: cancellationToken);

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
            Brand = product.Brand,
            Weight = product.Weight,
            Ingredients = product.Ingredients,
            StorageInstructions = product.StorageInstructions,
            ExpirationInfo = product.ExpirationInfo,
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
        }, "Producto actualizado exitosamente");
    }
}
