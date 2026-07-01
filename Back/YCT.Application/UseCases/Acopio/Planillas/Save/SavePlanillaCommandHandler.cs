using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Application.UseCases.Acopio.Planillas.GetById;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Planillas.Save;

public class SavePlanillaCommandHandler : IRequestHandler<SavePlanillaCommand, ResponseBase<PlanillaDto>>
{
    private readonly IGenericRepository<Ruta> _rutaRepository;
    private readonly IGenericRepository<Recogida> _recogidaRepository;
    private readonly IGenericRepository<Camion> _camionRepository;
    private readonly IGenericRepository<Conductor> _conductorRepository;
    private readonly IGenericRepository<Granjero> _granjeroRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;
    private readonly IMediator _mediator;

    public SavePlanillaCommandHandler(
        IGenericRepository<Ruta> rutaRepository,
        IGenericRepository<Recogida> recogidaRepository,
        IGenericRepository<Camion> camionRepository,
        IGenericRepository<Conductor> conductorRepository,
        IGenericRepository<Granjero> granjeroRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit,
        IMediator mediator)
    {
        _rutaRepository = rutaRepository;
        _recogidaRepository = recogidaRepository;
        _camionRepository = camionRepository;
        _conductorRepository = conductorRepository;
        _granjeroRepository = granjeroRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _mediator = mediator;
    }

    public async Task<ResponseBase<PlanillaDto>> Handle(SavePlanillaCommand request, CancellationToken cancellationToken)
    {
        // ===== Validaciones =====
        if (string.IsNullOrWhiteSpace(request.Codigo))
            return ResponseBase<PlanillaDto>.Fail("El código de la ruta es obligatorio");

        var camion = await _camionRepository.GetByIdAsync(request.CamionId);
        if (camion == null) return ResponseBase<PlanillaDto>.Fail("Camión no encontrado");

        var conductor = await _conductorRepository.GetByIdAsync(request.ConductorId);
        if (conductor == null) return ResponseBase<PlanillaDto>.Fail("Conductor no encontrado");

        foreach (var item in request.Items)
        {
            if (item.Cantinas < 0) return ResponseBase<PlanillaDto>.Fail("Las cantinas no pueden ser negativas");
            if (item.SaldoLitros < 0 || item.SaldoLitros >= 40)
                return ResponseBase<PlanillaDto>.Fail("El saldo debe estar entre 0 y 39.99 litros (si es 40 o más es una cantina más)");
            var g = await _granjeroRepository.GetByIdAsync(item.GranjeroId);
            if (g == null) return ResponseBase<PlanillaDto>.Fail($"Granjero {item.GranjeroId} no encontrado");
        }

        // ===== Upsert Ruta =====
        Ruta ruta;
        bool isNew = !request.Id.HasValue || request.Id == 0;

        if (isNew)
        {
            ruta = new Ruta
            {
                Codigo = request.Codigo.Trim().ToUpperInvariant(),
                Fecha = request.Fecha.Date,
                CamionId = request.CamionId,
                ConductorId = request.ConductorId,
                AsistenteId = request.AsistenteId,
                HoraSalida = request.HoraSalida,
                HoraLlegadaPlanta = request.HoraLlegadaPlanta,
                HoraDescargue = request.HoraDescargue,
                Observaciones = request.Observaciones?.Trim(),
                Status = "EnProgreso"
            };
            await _rutaRepository.AddAsync(ruta);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var existing = await _rutaRepository.GetByIdAsync(request.Id!.Value);
            if (existing == null) return ResponseBase<PlanillaDto>.Fail("Planilla no encontrada");

            existing.Codigo = request.Codigo.Trim().ToUpperInvariant();
            existing.Fecha = request.Fecha.Date;
            existing.CamionId = request.CamionId;
            existing.ConductorId = request.ConductorId;
            existing.AsistenteId = request.AsistenteId;
            existing.HoraSalida = request.HoraSalida;
            existing.HoraLlegadaPlanta = request.HoraLlegadaPlanta;
            existing.HoraDescargue = request.HoraDescargue;
            existing.Observaciones = request.Observaciones?.Trim();
            existing.UpdatedAt = DateTime.UtcNow;
            await _rutaRepository.UpdateAsync(existing);
            ruta = existing;
        }

        // ===== Sync recogidas (estrategia simple: borra y re-crea) =====
        var existentes = (await _recogidaRepository.FindAsync(r => r.RutaId == ruta.Id)).ToList();
        foreach (var ex in existentes) await _recogidaRepository.DeleteAsync(ex);

        decimal totalLitros = 0;
        int totalCantinas = 0;
        foreach (var item in request.Items)
        {
            var litros = item.Cantinas * 40m + item.SaldoLitros;
            var recogida = new Recogida
            {
                RutaId = ruta.Id,
                GranjeroId = item.GranjeroId,
                GranjeroCodigoId = item.GranjeroCodigoId,
                Fecha = item.Fecha.Date == DateTime.MinValue ? ruta.Fecha : item.Fecha.Date,
                CantinasChofer = item.Cantinas,
                SaldoChofer = item.SaldoLitros,
                LitrosChofer = litros,
                LitrosRegaladosChofer = item.LitrosRegaladosChofer,
                Observacion = item.Observacion,
                EstadoVista = item.EstadoVista,
                EstadoOlor = item.EstadoOlor,
                EstadoSabor = item.EstadoSabor,
                RecogidoAt = DateTime.UtcNow
            };
            await _recogidaRepository.AddAsync(recogida);
            totalLitros += litros;
            totalCantinas += item.Cantinas;
        }

        ruta.TotalLitrosChofer = totalLitros;
        ruta.UpdatedAt = DateTime.UtcNow;

        // Auto-transición: si hay items y no está conciliada/anulada, pasa a EsperandoDescargue
        if (request.Items.Count > 0 && ruta.Status != "Conciliada" && ruta.Status != "Anulada"
            && ruta.Status != "PendienteAutorizacion")
        {
            ruta.Status = "EsperandoDescargue";
        }

        await _rutaRepository.UpdateAsync(ruta);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(isNew ? "Create" : "Update", "Planilla", ruta.Id,
            $"Planilla {(isNew ? "creada" : "actualizada")}: {ruta.Codigo} {ruta.Fecha:dd/MM/yyyy} · {totalLitros} L · {request.Items.Count} recogidas",
            new { ruta.Codigo, ruta.Fecha, ruta.CamionId, ruta.ConductorId, totalLitros, items = request.Items.Count },
            ct: cancellationToken);

        var result = await _mediator.Send(new GetPlanillaByIdQuery(ruta.Id), cancellationToken);
        return ResponseBase<PlanillaDto>.Ok(result.Data!, isNew ? "Planilla creada" : "Planilla actualizada");
    }
}
