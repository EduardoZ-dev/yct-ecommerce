using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.GetAll;

public record GetAllClientesTercerosQuery() : IRequest<ResponseBase<List<ClienteTerceroDto>>>;
