using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Tinas.UpdatePlanta;

public class UpdateTinaPlantaCommandHandler : IRequestHandler<UpdateTinaPlantaCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<TinaPlanta> _plantaRepo;
    private readonly IGenericRepository<TinaMovimiento> _movRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateTinaPlantaCommandHandler(
        IGenericRepository<TinaPlanta> plantaRepo,
        IGenericRepository<TinaMovimiento> movRepo,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _plantaRepo = plantaRepo;
        _movRepo = movRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ResponseBase<bool>> Handle(UpdateTinaPlantaCommand request, CancellationToken cancellationToken)
    {
        if (request.Cantidad < 0)
            return ResponseBase<bool>.Fail("La cantidad de tinas no puede ser negativa");

        var planta = await _plantaRepo.GetByIdAsync(1);
        if (planta == null)
        {
            planta = new TinaPlanta { Id = 1, Cantidad = 0 };
            await _plantaRepo.AddAsync(planta);
        }

        var anterior = planta.Cantidad;
        if (anterior == request.Cantidad && string.IsNullOrWhiteSpace(request.Observacion))
            return ResponseBase<bool>.Ok(true, "Sin cambios");

        planta.Cantidad = request.Cantidad;
        planta.UpdatedAt = DateTime.UtcNow;
        await _plantaRepo.UpdateAsync(planta);

        await _movRepo.AddAsync(new TinaMovimiento
        {
            GranjeroCodigoId = null,
            EsPlanta = true,
            CantidadAnterior = anterior,
            CantidadNueva = request.Cantidad,
            Observacion = request.Observacion?.Trim(),
            UsuarioNombre = _currentUser.FullName ?? _currentUser.Username
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ResponseBase<bool>.Ok(true, "Planta actualizada");
    }
}
