using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Tinas.GetAll;

public record GetTinasQuery() : IRequest<ResponseBase<TinasOverviewDto>>;
