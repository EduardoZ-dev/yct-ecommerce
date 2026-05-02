using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Users.GetAll;

public record GetAllUsersQuery() : IRequest<ResponseBase<List<UserDto>>>;
