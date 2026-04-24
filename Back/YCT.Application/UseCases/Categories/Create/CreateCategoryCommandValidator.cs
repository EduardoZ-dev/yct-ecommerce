using FluentValidation;

namespace YCT.Application.UseCases.Categories.Create;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("El nombre es requerido (máx 100 caracteres)");
    }
}
