using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Recepcion.Login;

/// <summary>Login de la tablet de recepción: usuario + clave → JWT rol Recepcion (token largo).</summary>
public class RecepcionLoginCommand : IRequest<ResponseBase<RecepcionLoginResponse>>
{
    public string Usuario { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
}
