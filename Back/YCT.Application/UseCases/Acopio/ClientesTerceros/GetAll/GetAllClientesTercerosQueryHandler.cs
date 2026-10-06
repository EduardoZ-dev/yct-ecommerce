using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.GetAll;

public class GetAllClientesTercerosQueryHandler : IRequestHandler<GetAllClientesTercerosQuery, ResponseBase<List<ClienteTerceroDto>>>
{
    private readonly IGenericRepository<ClienteTercero> _repository;
    private readonly IGenericRepository<EntregaTercero> _entregaRepository;

    public GetAllClientesTercerosQueryHandler(
        IGenericRepository<ClienteTercero> repository,
        IGenericRepository<EntregaTercero> entregaRepository)
    {
        _repository = repository;
        _entregaRepository = entregaRepository;
    }

    public async Task<ResponseBase<List<ClienteTerceroDto>>> Handle(GetAllClientesTercerosQuery request, CancellationToken cancellationToken)
    {
        var clientes = await _repository.GetAllAsync();
        // Una sola lectura de entregas agrupada por cliente: evita una consulta por fila.
        var entregasPorCliente = (await _entregaRepository.GetAllAsync()).ToLookup(e => e.ClienteTerceroId);

        var dtos = clientes
            .OrderByDescending(c => c.IsActive)
            .ThenBy(c => c.NombreCompleto)
            .Select(c => ClientesTercerosMapper.ToDto(c, entregasPorCliente[c.Id]))
            .ToList();

        return ResponseBase<List<ClienteTerceroDto>>.Ok(dtos);
    }
}
