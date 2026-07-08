using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Tinas.UpdateFinca;

public class UpdateTinaFincaCommandHandler : IRequestHandler<UpdateTinaFincaCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<GranjeroCodigo> _codigoRepo;
    private readonly IGenericRepository<TinaMovimiento> _movRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateTinaFincaCommandHandler(
        IGenericRepository<GranjeroCodigo> codigoRepo,
        IGenericRepository<TinaMovimiento> movRepo,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _codigoRepo = codigoRepo;
        _movRepo = movRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ResponseBase<bool>> Handle(UpdateTinaFincaCommand request, CancellationToken cancellationToken)
    {
        if (request.Cantidad < 0)
            return ResponseBase<bool>.Fail("La cantidad de tinas no puede ser negativa");

        var codigo = await _codigoRepo.GetByIdAsync(request.CodigoId);
        if (codigo == null)
            return ResponseBase<bool>.Fail("Finca (código) no encontrada");

        var anterior = codigo.TinasYct;
        if (anterior == request.Cantidad && string.IsNullOrWhiteSpace(request.Observacion))
            return ResponseBase<bool>.Ok(true, "Sin cambios");

        codigo.TinasYct = request.Cantidad;
        codigo.UpdatedAt = DateTime.UtcNow;
        await _codigoRepo.UpdateAsync(codigo);

        await _movRepo.AddAsync(new TinaMovimiento
        {
            GranjeroCodigoId = codigo.Id,
            EsPlanta = false,
            CantidadAnterior = anterior,
            CantidadNueva = request.Cantidad,
            Observacion = request.Observacion?.Trim(),
            UsuarioNombre = _currentUser.FullName ?? _currentUser.Username
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ResponseBase<bool>.Ok(true, "Tinas actualizadas");
    }
}
