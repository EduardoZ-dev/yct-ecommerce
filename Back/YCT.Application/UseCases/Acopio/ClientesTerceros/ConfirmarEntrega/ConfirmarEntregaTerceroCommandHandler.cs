using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.ConfirmarEntrega;

public class ConfirmarEntregaTerceroCommandHandler : IRequestHandler<ConfirmarEntregaTerceroCommand, ResponseBase<EntregaTerceroDto>>
{
    private readonly IGenericRepository<EntregaTercero> _entregaRepository;
    private readonly IGenericRepository<Ruta> _rutaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ConfirmarEntregaTerceroCommandHandler(
        IGenericRepository<EntregaTercero> entregaRepository,
        IGenericRepository<Ruta> rutaRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _entregaRepository = entregaRepository;
        _rutaRepository = rutaRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<ResponseBase<EntregaTerceroDto>> Handle(ConfirmarEntregaTerceroCommand request, CancellationToken cancellationToken)
    {
        // Se incluye el cliente para no pedir su nombre en una segunda consulta.
        var coincidencias = await _entregaRepository.FindAsync(e => e.Id == request.Id, e => e.ClienteTercero);
        var entrega = coincidencias.FirstOrDefault();
        if (entrega == null)
            return ResponseBase<EntregaTerceroDto>.Fail("Entrega no encontrada");

        // Idempotente: la tablet puede reenviar la confirmación sin alterar quién ni cuándo confirmó.
        if (entrega.ConfirmadaEnPlantaAt.HasValue)
            return ResponseBase<EntregaTerceroDto>.Ok(await ToDtoAsync(entrega), "La entrega ya estaba confirmada");

        entrega.ConfirmadaEnPlantaAt = DateTime.UtcNow;
        entrega.ConfirmadaPorUserId = _currentUser.UserId;
        entrega.ConfirmadaPorNombre = _currentUser.FullName ?? _currentUser.Username;
        entrega.UpdatedAt = DateTime.UtcNow;
        await _entregaRepository.UpdateAsync(entrega);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("ConfirmarEntrega", "EntregaTercero", entrega.Id,
            $"Llegada confirmada en planta: {entrega.ClienteTercero.NombreCompleto}, {entrega.Litros:0.##} L ({entrega.Fecha:yyyy-MM-dd})",
            new { entrega.ClienteTerceroId, entrega.Fecha, entrega.Litros, entrega.ConfirmadaEnPlantaAt, entrega.ConfirmadaPorNombre },
            ct: cancellationToken);

        return ResponseBase<EntregaTerceroDto>.Ok(await ToDtoAsync(entrega), "Llegada confirmada");
    }

    /// <summary>Resuelve el código de la ruta cuando la entrega llegó en un descargue (registro desde tablet).</summary>
    private async Task<EntregaTerceroDto> ToDtoAsync(EntregaTercero entrega)
    {
        var ruta = entrega.RutaId.HasValue ? await _rutaRepository.GetByIdAsync(entrega.RutaId.Value) : null;
        return ClientesTercerosMapper.ToDto(entrega, entrega.ClienteTercero.NombreCompleto, ruta?.Codigo);
    }
}
