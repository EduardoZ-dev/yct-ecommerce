using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Users.Update;

public record UpdateUserCommand(
    int Id,
    string FullName,
    string? Email,
    string? Phone,
    string Role,
    bool IsActive,
    string? NewPassword
) : IRequest<ResponseBase<UserDto>>;
