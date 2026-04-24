using FluentValidation;

namespace YCT.Application.UseCases.Auth.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("El usuario es requerido");
        RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es requerida");
    }
}
