using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Auth.Login;

public record LoginCommand(string Username, string Password) : IRequest<ResponseBase<LoginResponse>>;
