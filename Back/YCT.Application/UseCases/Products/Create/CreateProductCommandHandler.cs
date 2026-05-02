using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Products.Create;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ResponseBase<ProductDto>>
{
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public CreateProductCommandHandler(
        IGenericRepository<Product> productRepository,
        IGenericRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            return ResponseBase<ProductDto>.Fail("La categoría especificada no existe");

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            ImageUrl = request.ImageUrl,
            CategoryId = request.CategoryId,
            Brand = request.Brand,
            Weight = request.Weight,
            Ingredients = request.Ingredients,
            StorageInstructions = request.StorageInstructions,
            ExpirationInfo = request.ExpirationInfo,
            ServingSize = request.ServingSize,
            Calories = request.Calories,
            TotalFat = request.TotalFat,
            SaturatedFat = request.SaturatedFat,
            Cholesterol = request.Cholesterol,
            Sodium = request.Sodium,
            TotalCarbs = request.TotalCarbs,
            Sugars = request.Sugars,
            Protein = request.Protein,
            Calcium = request.Calcium,
            Iron = request.Iron,
            VitaminD = request.VitaminD
        };

        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Create", "Product", product.Id,
            $"Producto creado: {product.Name}",
            new { product.Name, product.Price, product.Stock, Category = category.Name },
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
            CategoryName = category.Name,
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
        }, "Producto creado exitosamente");
    }
}
