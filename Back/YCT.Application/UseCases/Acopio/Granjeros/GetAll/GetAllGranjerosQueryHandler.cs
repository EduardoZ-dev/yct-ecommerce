using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Granjeros.GetAll;

public class GetAllGranjerosQueryHandler : IRequestHandler<GetAllGranjerosQuery, ResponseBase<List<GranjeroDto>>>
{
    private readonly IGenericRepository<Granjero> _repository;

    public GetAllGranjerosQueryHandler(IGenericRepository<Granjero> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<List<GranjeroDto>>> Handle(GetAllGranjerosQuery request, CancellationToken cancellationToken)
    {
        var all = (await _repository.GetAllAsync()).ToList();
        var dtos = all
            .OrderByDescending(g => g.IsActive)
            .ThenBy(g => g.Numero)
            .Select(g => new GranjeroDto
            {
                Id = g.Id,
                Numero = g.Numero,
                NombreCompleto = g.NombreCompleto,
                Cedula = g.Cedula,
                Telefono = g.Telefono,
                Finca = g.Finca,
                Vereda = g.Vereda,
                Municipio = g.Municipio,
                PrecioLitro = g.PrecioLitro,
                PromedioDiario = g.PromedioDiario,
                Notas = g.Notas,
                IsActive = g.IsActive,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            }).ToList();

        return ResponseBase<List<GranjeroDto>>.Ok(dtos);
    }
}
