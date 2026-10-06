using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.Delete;

public class DeleteClienteTerceroCommandHandler : IRequestHandler<DeleteClienteTerceroCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<ClienteTercero> _repository;
    private readonly IGenericRepository<EntregaTercero> _entregaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public DeleteClienteTerceroCommandHandler(
        IGenericRepository<ClienteTercero> repository,
        IGenericRepository<EntregaTercero> entregaRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _entregaRepository = entregaRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<bool>> Handle(DeleteClienteTerceroCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null)
            return ResponseBase<bool>.Fail("Cliente tercero no encontrado");

        // Las entregas son leche que sí entró a planta: el histórico se conserva y el cliente se inactiva.
        var totalEntregas = await _entregaRepository.CountAsync(e => e.ClienteTerceroId == request.Id);
        if (totalEntregas > 0)
            return ResponseBase<bool>.Fail("No se puede eliminar el cliente: tiene entregas registradas. Márcalo como inactivo.");

        await _repository.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Delete", "ClienteTercero", entity.Id,
            $"Cliente tercero eliminado: {entity.NombreCompleto}",
            new { entity.NombreCompleto, entity.Cedula },
            ct: cancellationToken);

        return ResponseBase<bool>.Ok(true, "Cliente tercero eliminado");
    }
}
