using System.Security.Cryptography;
using System.Text;
using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Conductores.Save;

public class SaveConductorCommandHandler : IRequestHandler<SaveConductorCommand, ResponseBase<ConductorDto>>
{
    private readonly IGenericRepository<Conductor> _repository;
    private readonly IGenericRepository<Camion> _camionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public SaveConductorCommandHandler(
        IGenericRepository<Conductor> repository,
        IGenericRepository<Camion> camionRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _camionRepository = camionRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<ConductorDto>> Handle(SaveConductorCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NombreCompleto))
            return ResponseBase<ConductorDto>.Fail("El nombre completo es obligatorio");

        if (request.CamionPreferidoId.HasValue)
        {
            var camion = await _camionRepository.GetByIdAsync(request.CamionPreferidoId.Value);
            if (camion == null)
                return ResponseBase<ConductorDto>.Fail("Camión preferido no encontrado");
        }

        Conductor entity;
        bool isNew = !request.Id.HasValue || request.Id == 0;

        if (isNew)
        {
            entity = new Conductor
            {
                NombreCompleto = request.NombreCompleto.Trim(),
                Cedula = request.Cedula?.Trim(),
                Telefono = request.Telefono?.Trim(),
                CamionPreferidoId = request.CamionPreferidoId,
                UserId = request.UserId,
                IsActive = request.IsActive
            };
            if (request.Pin != null)
                entity.PinHash = string.IsNullOrWhiteSpace(request.Pin) ? null : HashPin(request.Pin);
            await _repository.AddAsync(entity);
        }
        else
        {
            var existing = await _repository.GetByIdAsync(request.Id!.Value);
            if (existing == null)
                return ResponseBase<ConductorDto>.Fail("Conductor no encontrado");

            existing.NombreCompleto = request.NombreCompleto.Trim();
            existing.Cedula = request.Cedula?.Trim();
            existing.Telefono = request.Telefono?.Trim();
            existing.CamionPreferidoId = request.CamionPreferidoId;
            existing.UserId = request.UserId;
            existing.IsActive = request.IsActive;
            if (request.Pin != null)
                existing.PinHash = string.IsNullOrWhiteSpace(request.Pin) ? null : HashPin(request.Pin);
            existing.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(existing);
            entity = existing;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        string? camionNombre = null;
        if (entity.CamionPreferidoId.HasValue)
        {
            var c = await _camionRepository.GetByIdAsync(entity.CamionPreferidoId.Value);
            camionNombre = c?.Nombre;
        }

        await _audit.LogAsync(isNew ? "Create" : "Update", "Conductor", entity.Id,
            $"Conductor {(isNew ? "creado" : "actualizado")}: {entity.NombreCompleto}",
            new { entity.NombreCompleto, entity.Cedula, entity.CamionPreferidoId, entity.IsActive },
            ct: cancellationToken);

        return ResponseBase<ConductorDto>.Ok(new ConductorDto
        {
            Id = entity.Id,
            NombreCompleto = entity.NombreCompleto,
            Cedula = entity.Cedula,
            Telefono = entity.Telefono,
            CamionPreferidoId = entity.CamionPreferidoId,
            CamionPreferidoNombre = camionNombre,
            UserId = entity.UserId,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        }, isNew ? "Conductor creado" : "Conductor actualizado");
    }

    private static string HashPin(string pin) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pin.Trim()))).ToLowerInvariant();
}
