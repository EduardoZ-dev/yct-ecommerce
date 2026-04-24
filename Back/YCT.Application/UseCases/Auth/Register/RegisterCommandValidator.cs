using FluentValidation;

namespace YCT.Application.UseCases.Auth.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).WithMessage("El usuario debe tener al menos 3 caracteres");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
        RuleFor(x => x.FullName).NotEmpty().WithMessage("El nombre completo es requerido");
    }
}
