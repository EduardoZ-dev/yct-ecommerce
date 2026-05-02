using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Distributors.Delete;

public class DeleteDistributorCommandHandler : IRequestHandler<DeleteDistributorCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<Distributor> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public DeleteDistributorCommandHandler(
        IGenericRepository<Distributor> repository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<bool>> Handle(DeleteDistributorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null)
            return ResponseBase<bool>.Fail("Distribuidor no encontrado");

        // Soft delete: solo desactivar (preservar historial de pedidos)
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Delete", "Distributor", entity.Id,
            $"Distribuidor desactivado: {entity.Name}",
            new { entity.Name, entity.VehicleType },
            ct: cancellationToken);

        return ResponseBase<bool>.Ok(true, "Distribuidor desactivado");
    }
}
