using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Asistentes.GetAll;

public record GetAllAsistentesQuery() : IRequest<ResponseBase<List<AsistenteDto>>>;
