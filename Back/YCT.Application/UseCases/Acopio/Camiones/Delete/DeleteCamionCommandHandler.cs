using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Camiones.Delete;

public class DeleteCamionCommandHandler : IRequestHandler<DeleteCamionCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<Camion> _repository;
    private readonly IGenericRepository<Ruta> _rutaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public DeleteCamionCommandHandler(
        IGenericRepository<Camion> repository,
        IGenericRepository<Ruta> rutaRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _rutaRepository = rutaRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<bool>> Handle(DeleteCamionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null)
            return ResponseBase<bool>.Fail("Camión no encontrado");

        var conRutas = await _rutaRepository.FindAsync(r => r.CamionId == request.Id);
        if (conRutas.Any())
            return ResponseBase<bool>.Fail("No se puede eliminar el camión: tiene rutas asociadas. Marca como Inactivo.");

        await _repository.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Delete", "Camion", entity.Id,
            $"Camión eliminado: {entity.Nombre}",
            new { entity.Nombre, entity.Placa },
            ct: cancellationToken);

        return ResponseBase<bool>.Ok(true, "Camión eliminado");
    }
}
