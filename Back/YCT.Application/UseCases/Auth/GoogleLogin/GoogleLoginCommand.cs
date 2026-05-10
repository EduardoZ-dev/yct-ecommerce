using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Auth.GoogleLogin;

public record GoogleLoginCommand(string IdToken) : IRequest<ResponseBase<LoginResponse>>;
