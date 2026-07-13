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
    private readonly IGenericRepository<GranjeroCodigo> _codigoRepository;
    private readonly IGenericRepository<Asistente> _asistenteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;
    private readonly IMediator _mediator;

    public SavePlanillaCommandHandler(
        IGenericRepository<Ruta> rutaRepository,
        IGenericRepository<Recogida> recogidaRepository,
        IGenericRepository<Camion> camionRepository,
        IGenericRepository<Conductor> conductorRepository,
        IGenericRepository<Granjero> granjeroRepository,
        IGenericRepository<GranjeroCodigo> codigoRepository,
        IGenericRepository<Asistente> asistenteRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit,
        IMediator mediator)
    {
        _rutaRepository = rutaRepository;
        _recogidaRepository = recogidaRepository;
        _camionRepository = camionRepository;
        _conductorRepository = conductorRepository;
        _granjeroRepository = granjeroRepository;
        _codigoRepository = codigoRepository;
        _asistenteRepository = asistenteRepository;
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

        // IMPORTANTE: este handler hace DOS SaveChanges (la Ruta primero, para obtener su Id,
        // y luego las Recogidas) y no hay transacción. Si el segundo revienta, la Ruta ya
        // quedó commiteada → ruta fantasma vacía + 500 → la tablet reintenta para siempre y
        // la planilla nunca se recupera. Por eso TODO lo que pueda hacer fallar el segundo
        // SaveChanges (FKs y límites de columna) se valida ANTES de tocar la base.

        if (request.AsistenteId.HasValue &&
            await _asistenteRepository.GetByIdAsync(request.AsistenteId.Value) == null)
            return ResponseBase<PlanillaDto>.Fail($"Asistente {request.AsistenteId} no encontrado");

        foreach (var item in request.Items)
        {
            if (item.Cantinas < 0) return ResponseBase<PlanillaDto>.Fail("Las cantinas no pueden ser negativas");
            if (item.SaldoLitros < 0 || item.SaldoLitros >= 40)
                return ResponseBase<PlanillaDto>.Fail("El saldo debe estar entre 0 y 39.99 litros (si es 40 o más es una cantina más)");
            if (item.LitrosRegaladosChofer < 0 || item.LitrosRegaladosChofer > 9999)
                return ResponseBase<PlanillaDto>.Fail("Los litros regalados al chofer están fuera de rango");

            var g = await _granjeroRepository.GetByIdAsync(item.GranjeroId);
            if (g == null) return ResponseBase<PlanillaDto>.Fail($"Granjero {item.GranjeroId} no encontrado");

            // El chofer trabaja con una lista cacheada: un código borrado en el panel llegaría
            // aquí y violaría la FK justo en el segundo SaveChanges.
            if (item.GranjeroCodigoId.HasValue &&
                await _codigoRepository.GetByIdAsync(item.GranjeroCodigoId.Value) == null)
                return ResponseBase<PlanillaDto>.Fail(
                    $"La finca/código {item.GranjeroCodigoId} ya no existe. Actualiza la lista de granjeros.");
        }

        // Recortes defensivos: el texto libre no puede exceder el largo de la columna
        // (Recogida.Observacion y Ruta.Observaciones son de 500).
        const int MaxObs = 500;
        static string? Recortar(string? s, int max) =>
            string.IsNullOrWhiteSpace(s) ? null : s.Trim()[..Math.Min(s.Trim().Length, max)];

        foreach (var item in request.Items)
            item.Observacion = Recortar(item.Observacion, MaxObs);

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
                Observaciones = Recortar(request.Observaciones, MaxObs),
                ChoferUuid = request.ChoferUuid,
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
            existing.Observaciones = Recortar(request.Observaciones, MaxObs);
            if (!string.IsNullOrWhiteSpace(request.ChoferUuid)) existing.ChoferUuid = request.ChoferUuid;
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
                Orden = item.Orden,
                CapturadoAt = item.CapturadoAt,
                GpsLat = item.GpsLat,
                GpsLng = item.GpsLng,
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
