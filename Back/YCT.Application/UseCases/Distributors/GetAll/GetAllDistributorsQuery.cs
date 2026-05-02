using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Distributors.GetAll;

public record GetAllDistributorsQuery() : IRequest<ResponseBase<List<DistributorDto>>>;
