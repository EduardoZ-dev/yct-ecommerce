using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Granjeros.GetAll;

public record GetAllGranjerosQuery() : IRequest<ResponseBase<List<GranjeroDto>>>;
