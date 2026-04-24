using FluentValidation;

namespace YCT.Application.UseCases.Orders.Create;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("El usuario es requerido");
        RuleFor(x => x.Details).NotEmpty().WithMessage("La orden debe tener al menos un producto");
        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId).GreaterThan(0).WithMessage("Producto inválido");
            detail.RuleFor(d => d.Quantity).GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");
        });
    }
}
