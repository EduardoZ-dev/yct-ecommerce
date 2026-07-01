using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Granjeros.GetAll;

public class GetAllGranjerosQueryHandler : IRequestHandler<GetAllGranjerosQuery, ResponseBase<List<GranjeroDto>>>
{
    private readonly IGenericRepository<Granjero> _repository;
    private readonly IGenericRepository<GranjeroCodigo> _codigoRepo;

    public GetAllGranjerosQueryHandler(
        IGenericRepository<Granjero> repository,
        IGenericRepository<GranjeroCodigo> codigoRepo)
    {
        _repository = repository;
        _codigoRepo = codigoRepo;
    }

    public async Task<ResponseBase<List<GranjeroDto>>> Handle(GetAllGranjerosQuery request, CancellationToken cancellationToken)
    {
        var all = (await _repository.GetAllAsync()).ToList();
        var codigosByGranjero = (await _codigoRepo.FindAsync(c => c.IsActive))
            .GroupBy(c => c.GranjeroId)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Codigo).ToList());
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
                UpdatedAt = g.UpdatedAt,
                Codigos = (codigosByGranjero.TryGetValue(g.Id, out var cs) ? cs : new List<GranjeroCodigo>())
                    .Select(c => new GranjeroCodigoDto
                    {
                        Id = c.Id,
                        Codigo = c.Codigo,
                        Finca = c.Finca,
                        TinasYct = c.TinasYct
                    }).ToList(),
                TinasYctTotal = (codigosByGranjero.TryGetValue(g.Id, out var cs2) ? cs2 : new List<GranjeroCodigo>())
                    .Sum(c => c.TinasYct)
            }).ToList();

        return ResponseBase<List<GranjeroDto>>.Ok(dtos);
    }
}
