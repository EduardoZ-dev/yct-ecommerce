using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.DeleteEntrega;

public class DeleteEntregaTerceroCommandHandler : IRequestHandler<DeleteEntregaTerceroCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<EntregaTercero> _entregaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public DeleteEntregaTerceroCommandHandler(
        IGenericRepository<EntregaTercero> entregaRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _entregaRepository = entregaRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<bool>> Handle(DeleteEntregaTerceroCommand request, CancellationToken cancellationToken)
    {
        // Se incluye el cliente para dejar su nombre en la auditoría, no solo el id.
        var coincidencias = await _entregaRepository.FindAsync(e => e.Id == request.Id, e => e.ClienteTercero);
        var entrega = coincidencias.FirstOrDefault();
        if (entrega == null)
            return ResponseBase<bool>.Fail("Entrega no encontrada");

        await _entregaRepository.DeleteAsync(entrega);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Delete", "EntregaTercero", entrega.Id,
            $"Entrega de tercero eliminada: {entrega.ClienteTercero.NombreCompleto}, {entrega.Litros:0.##} L ({entrega.Fecha:yyyy-MM-dd})",
            new { entrega.ClienteTerceroId, entrega.Fecha, entrega.Cantinas, entrega.SaldoLitros, entrega.Litros, entrega.Origen, entrega.RutaId },
            ct: cancellationToken);

        return ResponseBase<bool>.Ok(true, "Entrega eliminada");
    }
}
