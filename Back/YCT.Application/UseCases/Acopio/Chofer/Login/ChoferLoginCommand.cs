using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.Chofer.Login;

/// <summary>Login de la app móvil del chofer: cédula + PIN.</summary>
public record ChoferLoginCommand(string Cedula, string Pin) : IRequest<ResponseBase<ChoferLoginResponse>>;

public class ChoferLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int ConductorId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public int? CamionPreferidoId { get; set; }
    public string? CamionPreferidoNombre { get; set; }
}
