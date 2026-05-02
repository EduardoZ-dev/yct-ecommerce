using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Distributors.Delete;

public record DeleteDistributorCommand(int Id) : IRequest<ResponseBase<bool>>;
