using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.Asistentes.Delete;

public record DeleteAsistenteCommand(int Id) : IRequest<ResponseBase<bool>>;
