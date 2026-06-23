using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Planillas.GetAll;

public class GetAllPlanillasQueryHandler : IRequestHandler<GetAllPlanillasQuery, ResponseBase<List<PlanillaHeaderDto>>>
{
    private readonly IGenericRepository<Ruta> _repository;
    private readonly IGenericRepository<Recogida> _recogidaRepository;

    public GetAllPlanillasQueryHandler(
        IGenericRepository<Ruta> repository,
        IGenericRepository<Recogida> recogidaRepository)
    {
        _repository = repository;
        _recogidaRepository = recogidaRepository;
    }

    public async Task<ResponseBase<List<PlanillaHeaderDto>>> Handle(GetAllPlanillasQuery request, CancellationToken cancellationToken)
    {
        var rutas = (await _repository.GetAllAsync(r => r.Camion, r => r.Conductor, r => r.Asistente!)).ToList();
        var recogidas = await _recogidaRepository.GetAllAsync();

        var totals = recogidas
            .GroupBy(r => r.RutaId)
            .ToDictionary(g => g.Key, g => new
            {
                TotalLitros = g.Sum(x => x.LitrosChofer),
                TotalCantinas = g.Sum(x => x.CantinasChofer),
                Count = g.Count()
            });

        var dtos = rutas
            .OrderByDescending(r => r.Fecha)
            .ThenByDescending(r => r.Id)
            .Select(r =>
            {
                totals.TryGetValue(r.Id, out var t);
                return new PlanillaHeaderDto
                {
                    Id = r.Id,
                    Codigo = r.Codigo,
                    Fecha = r.Fecha,
                    CamionId = r.CamionId,
                    CamionNombre = r.Camion?.Nombre ?? string.Empty,
                    ConductorId = r.ConductorId,
                    ConductorNombre = r.Conductor?.NombreCompleto ?? string.Empty,
                    AsistenteId = r.AsistenteId,
                    AsistenteNombre = r.Asistente?.NombreCompleto,
                    HoraSalida = r.HoraSalida,
                    HoraLlegadaPlanta = r.HoraLlegadaPlanta,
                    HoraDescargue = r.HoraDescargue,
                    TotalLitros = t?.TotalLitros ?? 0,
                    TotalLitrosPlanta = r.TotalLitrosPlanta,
                    DiferenciaTotal = r.DiferenciaTotal,
                    TotalCantinas = t?.TotalCantinas ?? 0,
                    TotalRecogidas = t?.Count ?? 0,
                    Status = r.Status,
                    Observaciones = r.Observaciones,
                    CreatedAt = r.CreatedAt
                };
            }).ToList();

        return ResponseBase<List<PlanillaHeaderDto>>.Ok(dtos);
    }
}
