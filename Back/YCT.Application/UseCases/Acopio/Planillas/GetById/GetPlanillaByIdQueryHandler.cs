using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Planillas.GetById;

public class GetPlanillaByIdQueryHandler : IRequestHandler<GetPlanillaByIdQuery, ResponseBase<PlanillaDto>>
{
    private readonly IGenericRepository<Ruta> _repository;
    private readonly IGenericRepository<Recogida> _recogidaRepository;
    private readonly IGenericRepository<Granjero> _granjeroRepository;

    public GetPlanillaByIdQueryHandler(
        IGenericRepository<Ruta> repository,
        IGenericRepository<Recogida> recogidaRepository,
        IGenericRepository<Granjero> granjeroRepository)
    {
        _repository = repository;
        _recogidaRepository = recogidaRepository;
        _granjeroRepository = granjeroRepository;
    }

    public async Task<ResponseBase<PlanillaDto>> Handle(GetPlanillaByIdQuery request, CancellationToken cancellationToken)
    {
        var rutas = await _repository.FindAsync(r => r.Id == request.Id, r => r.Camion, r => r.Conductor, r => r.Asistente!);
        var ruta = rutas.FirstOrDefault();
        if (ruta == null) return ResponseBase<PlanillaDto>.Fail("Planilla no encontrada");

        var recogidas = (await _recogidaRepository.FindAsync(r => r.RutaId == request.Id, r => r.Granjero)).ToList();
        var totalLitros = recogidas.Sum(r => r.LitrosChofer);
        var totalCantinas = recogidas.Sum(r => r.CantinasChofer);

        var items = recogidas
            .OrderBy(r => r.Id)
            .Select(r => new PlanillaItemDto
            {
                Id = r.Id,
                GranjeroId = r.GranjeroId,
                GranjeroNumero = r.Granjero?.Numero ?? 0,
                GranjeroNombre = r.Granjero?.NombreCompleto ?? string.Empty,
                Fecha = r.Fecha,
                Cantinas = r.CantinasChofer,
                SaldoLitros = r.SaldoChofer,
                TotalLitros = r.LitrosChofer
            }).ToList();

        var dto = new PlanillaDto
        {
            Id = ruta.Id,
            Codigo = ruta.Codigo,
            Fecha = ruta.Fecha,
            CamionId = ruta.CamionId,
            CamionNombre = ruta.Camion?.Nombre ?? string.Empty,
            ConductorId = ruta.ConductorId,
            ConductorNombre = ruta.Conductor?.NombreCompleto ?? string.Empty,
            AsistenteId = ruta.AsistenteId,
            AsistenteNombre = ruta.Asistente?.NombreCompleto,
            HoraSalida = ruta.HoraSalida,
            HoraLlegadaPlanta = ruta.HoraLlegadaPlanta,
            HoraDescargue = ruta.HoraDescargue,
            TotalLitros = totalLitros,
            TotalCantinas = totalCantinas,
            TotalRecogidas = items.Count,
            Status = ruta.Status,
            Observaciones = ruta.Observaciones,
            CreatedAt = ruta.CreatedAt,
            Items = items
        };

        return ResponseBase<PlanillaDto>.Ok(dto);
    }
}
