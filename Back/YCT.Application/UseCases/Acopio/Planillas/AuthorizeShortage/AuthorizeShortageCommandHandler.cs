using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Application.UseCases.Acopio.Planillas.GetById;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Planillas.AuthorizeShortage;

public class AuthorizeShortageCommandHandler : IRequestHandler<AuthorizeShortageCommand, ResponseBase<PlanillaDto>>
{
    private readonly IGenericRepository<Ruta> _rutaRepository;
    private readonly IGenericRepository<Notification> _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;
    private readonly IMediator _mediator;

    public AuthorizeShortageCommandHandler(
        IGenericRepository<Ruta> rutaRepository,
        IGenericRepository<Notification> notificationRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit,
        IMediator mediator)
    {
        _rutaRepository = rutaRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _mediator = mediator;
    }

    public async Task<ResponseBase<PlanillaDto>> Handle(AuthorizeShortageCommand request, CancellationToken cancellationToken)
    {
        var ruta = await _rutaRepository.GetByIdAsync(request.Id);
        if (ruta == null) return ResponseBase<PlanillaDto>.Fail("Planilla no encontrada");

        if (ruta.Status != "PendienteAutorizacion")
            return ResponseBase<PlanillaDto>.Fail($"La planilla no está pendiente de autorización (estado actual: {ruta.Status})");

        if (string.IsNullOrWhiteSpace(request.Motivo))
            return ResponseBase<PlanillaDto>.Fail("El motivo es obligatorio");

        ruta.Status = request.Approve ? "Conciliada" : "Anulada";
        var motivoTag = request.Approve ? "[Autorizado]" : "[Rechazado]";
        ruta.Observaciones = string.IsNullOrWhiteSpace(ruta.Observaciones)
            ? $"{motivoTag} {request.Motivo}"
            : $"{ruta.Observaciones}\n{motivoTag} {request.Motivo}";
        ruta.UpdatedAt = DateTime.UtcNow;

        await _rutaRepository.UpdateAsync(ruta);

        var notif = new Notification
        {
            Type = request.Approve ? "ShortageAuthorized" : "ShortageRejected",
            Title = $"Faltante {(request.Approve ? "autorizado" : "rechazado")} · {ruta.Codigo}",
            Message = $"{motivoTag} {request.Motivo}",
            UserId = null,
            PlanillaId = ruta.Id,
            IsRead = false
        };
        await _notificationRepository.AddAsync(notif);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(request.Approve ? "AuthorizeShortage" : "RejectShortage",
            "Planilla", ruta.Id,
            $"{motivoTag} {request.Motivo}",
            new { ruta.Codigo, approved = request.Approve, motivo = request.Motivo },
            ct: cancellationToken);

        var result = await _mediator.Send(new GetPlanillaByIdQuery(ruta.Id), cancellationToken);
        return ResponseBase<PlanillaDto>.Ok(result.Data!,
            request.Approve ? "Faltante autorizado · planilla conciliada" : "Faltante rechazado · planilla anulada");
    }
}
