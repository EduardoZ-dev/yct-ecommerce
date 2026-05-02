using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Distributors.Save;

public class SaveDistributorCommandHandler : IRequestHandler<SaveDistributorCommand, ResponseBase<DistributorDto>>
{
    private readonly IGenericRepository<Distributor> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public SaveDistributorCommandHandler(
        IGenericRepository<Distributor> repository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<DistributorDto>> Handle(SaveDistributorCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ResponseBase<DistributorDto>.Fail("El nombre es obligatorio");

        var validVehicles = new[] { "Moto", "Camioneta", "Camion", "Bicicleta", "Externo" };
        var vehicleType = validVehicles.Contains(request.VehicleType) ? request.VehicleType : "Moto";

        Distributor entity;
        bool isNew = !request.Id.HasValue || request.Id == 0;

        if (isNew)
        {
            entity = new Distributor
            {
                Name = request.Name.Trim(),
                Phone = request.Phone?.Trim(),
                VehicleType = vehicleType,
                VehiclePlate = request.VehiclePlate?.Trim(),
                Notes = request.Notes?.Trim(),
                IsActive = request.IsActive
            };
            await _repository.AddAsync(entity);
        }
        else
        {
            var existing = await _repository.GetByIdAsync(request.Id!.Value);
            if (existing == null)
                return ResponseBase<DistributorDto>.Fail("Distribuidor no encontrado");

            existing.Name = request.Name.Trim();
            existing.Phone = request.Phone?.Trim();
            existing.VehicleType = vehicleType;
            existing.VehiclePlate = request.VehiclePlate?.Trim();
            existing.Notes = request.Notes?.Trim();
            existing.IsActive = request.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(existing);
            entity = existing;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(isNew ? "Create" : "Update", "Distributor", entity.Id,
            $"Distribuidor {(isNew ? "creado" : "actualizado")}: {entity.Name} ({entity.VehicleType})",
            new { entity.Name, entity.VehicleType, entity.VehiclePlate, entity.IsActive },
            ct: cancellationToken);

        return ResponseBase<DistributorDto>.Ok(new DistributorDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Phone = entity.Phone,
            VehicleType = entity.VehicleType,
            VehiclePlate = entity.VehiclePlate,
            Notes = entity.Notes,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        }, isNew ? "Distribuidor creado" : "Distribuidor actualizado");
    }
}
