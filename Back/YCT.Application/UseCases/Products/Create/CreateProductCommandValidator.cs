using FluentValidation;

namespace YCT.Application.UseCases.Products.Create;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("El nombre es requerido (máx 200 caracteres)");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("El precio debe ser mayor a 0");
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Debe seleccionar una categoría");
    }
}
