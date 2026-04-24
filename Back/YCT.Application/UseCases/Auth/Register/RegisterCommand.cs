using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Auth.Register;

public record RegisterCommand(
    string Username,
    string Password,
    string FullName,
    string? Email,
    string? Phone) : IRequest<ResponseBase<UserDto>>;
