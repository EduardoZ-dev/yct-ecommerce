using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Tinas.GetAll;

public class GetTinasQueryHandler : IRequestHandler<GetTinasQuery, ResponseBase<TinasOverviewDto>>
{
    private readonly IGenericRepository<Granjero> _granjeroRepo;
    private readonly IGenericRepository<GranjeroCodigo> _codigoRepo;
    private readonly IGenericRepository<TinaMovimiento> _movRepo;
    private readonly IGenericRepository<TinaPlanta> _plantaRepo;

    public GetTinasQueryHandler(
        IGenericRepository<Granjero> granjeroRepo,
        IGenericRepository<GranjeroCodigo> codigoRepo,
        IGenericRepository<TinaMovimiento> movRepo,
        IGenericRepository<TinaPlanta> plantaRepo)
    {
        _granjeroRepo = granjeroRepo;
        _codigoRepo = codigoRepo;
        _movRepo = movRepo;
        _plantaRepo = plantaRepo;
    }

    public async Task<ResponseBase<TinasOverviewDto>> Handle(GetTinasQuery request, CancellationToken cancellationToken)
    {
        var granjeros = (await _granjeroRepo.GetAllAsync()).ToDictionary(g => g.Id);
        var codigos = (await _codigoRepo.FindAsync(c => c.IsActive)).ToList();

        var fincas = codigos
            .Select(c =>
            {
                granjeros.TryGetValue(c.GranjeroId, out var g);
                return new TinaFincaDto
                {
                    CodigoId = c.Id,
                    Codigo = c.Codigo,
                    Finca = c.Finca,
                    GranjeroId = c.GranjeroId,
                    GranjeroNumero = g?.Numero ?? 0,
                    GranjeroNombre = g?.NombreCompleto ?? "—",
                    TinasYct = c.TinasYct
                };
            })
            .OrderBy(f => f.GranjeroNumero).ThenBy(f => f.Codigo)
            .ToList();

        var planta = (await _plantaRepo.GetByIdAsync(1));
        var plantaDto = new TinaPlantaDto
        {
            Cantidad = planta?.Cantidad ?? 0,
            UpdatedAt = planta?.UpdatedAt
        };

        var codigosById = codigos.ToDictionary(c => c.Id);
        var historial = (await _movRepo.GetAllAsync())
            .OrderByDescending(m => m.CreatedAt)
            .Take(100)
            .Select(m =>
            {
                string? codigo = null, finca = null, granjero = null;
                if (m.GranjeroCodigoId.HasValue && codigosById.TryGetValue(m.GranjeroCodigoId.Value, out var c))
                {
                    codigo = c.Codigo;
                    finca = c.Finca;
                    granjeros.TryGetValue(c.GranjeroId, out var g);
                    granjero = g?.NombreCompleto;
                }
                return new TinaMovimientoDto
                {
                    Id = m.Id,
                    EsPlanta = m.EsPlanta,
                    CodigoId = m.GranjeroCodigoId,
                    Codigo = codigo,
                    Finca = finca,
                    Granjero = granjero,
                    CantidadAnterior = m.CantidadAnterior,
                    CantidadNueva = m.CantidadNueva,
                    Observacion = m.Observacion,
                    Usuario = m.UsuarioNombre,
                    Fecha = m.CreatedAt
                };
            })
            .ToList();

        var dto = new TinasOverviewDto
        {
            Fincas = fincas,
            Planta = plantaDto,
            Historial = historial,
            TotalTinasFincas = fincas.Sum(f => f.TinasYct)
        };
        return ResponseBase<TinasOverviewDto>.Ok(dto);
    }
}
