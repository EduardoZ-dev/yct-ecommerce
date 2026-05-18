using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Granjeros.Delete;

public class DeleteGranjeroCommandHandler : IRequestHandler<DeleteGranjeroCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<Granjero> _repository;
    private readonly IGenericRepository<Recogida> _recogidaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public DeleteGranjeroCommandHandler(
        IGenericRepository<Granjero> repository,
        IGenericRepository<Recogida> recogidaRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _recogidaRepository = recogidaRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<bool>> Handle(DeleteGranjeroCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null)
            return ResponseBase<bool>.Fail("Granjero no encontrado");

        var conRecogidas = await _recogidaRepository.FindAsync(r => r.GranjeroId == request.Id);
        if (conRecogidas.Any())
            return ResponseBase<bool>.Fail("No se puede eliminar el granjero: tiene recogidas históricas. Marca como inactivo.");

        await _repository.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Delete", "Granjero", entity.Id,
            $"Granjero eliminado: #{entity.Numero} {entity.NombreCompleto}",
            new { entity.Numero, entity.NombreCompleto },
            ct: cancellationToken);

        return ResponseBase<bool>.Ok(true, "Granjero eliminado");
    }
}
