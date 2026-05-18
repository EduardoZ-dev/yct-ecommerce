using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Conductores.GetAll;

public record GetAllConductoresQuery() : IRequest<ResponseBase<List<ConductorDto>>>;
