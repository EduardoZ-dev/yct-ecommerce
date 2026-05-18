using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Planillas.Delete;

public class DeletePlanillaCommandHandler : IRequestHandler<DeletePlanillaCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<Ruta> _repository;
    private readonly IGenericRepository<Recogida> _recogidaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public DeletePlanillaCommandHandler(
        IGenericRepository<Ruta> repository,
        IGenericRepository<Recogida> recogidaRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _recogidaRepository = recogidaRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<bool>> Handle(DeletePlanillaCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null) return ResponseBase<bool>.Fail("Planilla no encontrada");

        var items = await _recogidaRepository.FindAsync(r => r.RutaId == request.Id);
        foreach (var i in items) await _recogidaRepository.DeleteAsync(i);

        await _repository.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Delete", "Planilla", entity.Id,
            $"Planilla eliminada: {entity.Codigo} {entity.Fecha:dd/MM/yyyy}",
            new { entity.Codigo, entity.Fecha },
            ct: cancellationToken);

        return ResponseBase<bool>.Ok(true, "Planilla eliminada");
    }
}
