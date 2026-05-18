using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Camiones.GetAll;

public record GetAllCamionesQuery() : IRequest<ResponseBase<List<CamionDto>>>;
