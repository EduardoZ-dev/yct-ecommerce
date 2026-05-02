using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Users.Create;

public record CreateUserCommand(
    string Username,
    string Password,
    string FullName,
    string? Email,
    string? Phone,
    string Role
) : IRequest<ResponseBase<UserDto>>;
