using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Conductores.Delete;

public class DeleteConductorCommandHandler : IRequestHandler<DeleteConductorCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<Conductor> _repository;
    private readonly IGenericRepository<Ruta> _rutaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public DeleteConductorCommandHandler(
        IGenericRepository<Conductor> repository,
        IGenericRepository<Ruta> rutaRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _rutaRepository = rutaRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<bool>> Handle(DeleteConductorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null)
            return ResponseBase<bool>.Fail("Conductor no encontrado");

        var conRutas = await _rutaRepository.FindAsync(r => r.ConductorId == request.Id);
        if (conRutas.Any())
            return ResponseBase<bool>.Fail("No se puede eliminar el conductor: tiene rutas asociadas. Marca como inactivo.");

        await _repository.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Delete", "Conductor", entity.Id,
            $"Conductor eliminado: {entity.NombreCompleto}",
            new { entity.NombreCompleto, entity.Cedula },
            ct: cancellationToken);

        return ResponseBase<bool>.Ok(true, "Conductor eliminado");
    }
}
