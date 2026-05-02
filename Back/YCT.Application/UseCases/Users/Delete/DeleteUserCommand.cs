using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Users.Delete;

public record DeleteUserCommand(int Id) : IRequest<ResponseBase<bool>>;
