using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Application.UseCases.Acopio.Planillas.GetById;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Planillas.CorregirFecha;

public class CorregirFechaPlanillaCommandHandler
    : IRequestHandler<CorregirFechaPlanillaCommand, ResponseBase<PlanillaDto>>
{
    private readonly IGenericRepository<Ruta> _rutaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;
    private readonly IMediator _mediator;

    public CorregirFechaPlanillaCommandHandler(
        IGenericRepository<Ruta> rutaRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit,
        IMediator mediator)
    {
        _rutaRepository = rutaRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _mediator = mediator;
    }

    public async Task<ResponseBase<PlanillaDto>> Handle(CorregirFechaPlanillaCommand request, CancellationToken cancellationToken)
    {
        var ruta = await _rutaRepository.GetByIdAsync(request.Id);
        if (ruta == null) return ResponseBase<PlanillaDto>.Fail("Planilla no encontrada");

        // Idempotente: si ya no hay desfase marcado, no hay nada que corregir.
        if (ruta.FechaCapturaReal == null)
            return ResponseBase<PlanillaDto>.Fail("Esta planilla no tiene un desfase de fecha por corregir");

        var anterior = ruta.Fecha.Date;
        var nueva = ruta.FechaCapturaReal.Value.Date;

        ruta.Fecha = nueva;
        ruta.FechaCapturaReal = null; // corregido: se apaga el aviso
        ruta.UpdatedAt = DateTime.UtcNow;
        await _rutaRepository.UpdateAsync(ruta);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("CorregirFecha", "Planilla", ruta.Id,
            $"Fecha corregida: {anterior:dd/MM/yyyy} → {nueva:dd/MM/yyyy} (código {ruta.Codigo} sin cambios)",
            new { ruta.Codigo, Antes = anterior, Despues = nueva },
            ct: cancellationToken);

        var result = await _mediator.Send(new GetPlanillaByIdQuery(ruta.Id), cancellationToken);
        return ResponseBase<PlanillaDto>.Ok(result.Data!, $"Fecha corregida a {nueva:dd/MM/yyyy}");
    }
}
