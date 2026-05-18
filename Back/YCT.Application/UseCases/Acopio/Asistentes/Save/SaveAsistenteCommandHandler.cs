using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Asistentes.Save;

public class SaveAsistenteCommandHandler : IRequestHandler<SaveAsistenteCommand, ResponseBase<AsistenteDto>>
{
    private readonly IGenericRepository<Asistente> _repository;
    private readonly IGenericRepository<Camion> _camionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public SaveAsistenteCommandHandler(
        IGenericRepository<Asistente> repository,
        IGenericRepository<Camion> camionRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _camionRepository = camionRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<AsistenteDto>> Handle(SaveAsistenteCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NombreCompleto))
            return ResponseBase<AsistenteDto>.Fail("El nombre completo es obligatorio");

        if (request.CamionPreferidoId.HasValue)
        {
            var camion = await _camionRepository.GetByIdAsync(request.CamionPreferidoId.Value);
            if (camion == null)
                return ResponseBase<AsistenteDto>.Fail("Camión preferido no encontrado");
        }

        Asistente entity;
        bool isNew = !request.Id.HasValue || request.Id == 0;

        if (isNew)
        {
            entity = new Asistente
            {
                NombreCompleto = request.NombreCompleto.Trim(),
                Cedula = request.Cedula?.Trim(),
                Telefono = request.Telefono?.Trim(),
                CamionPreferidoId = request.CamionPreferidoId,
                IsActive = request.IsActive
            };
            await _repository.AddAsync(entity);
        }
        else
        {
            var existing = await _repository.GetByIdAsync(request.Id!.Value);
            if (existing == null)
                return ResponseBase<AsistenteDto>.Fail("Asistente no encontrado");

            existing.NombreCompleto = request.NombreCompleto.Trim();
            existing.Cedula = request.Cedula?.Trim();
            existing.Telefono = request.Telefono?.Trim();
            existing.CamionPreferidoId = request.CamionPreferidoId;
            existing.IsActive = request.IsActive;
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

        await _audit.LogAsync(isNew ? "Create" : "Update", "Asistente", entity.Id,
            $"Asistente {(isNew ? "creado" : "actualizado")}: {entity.NombreCompleto}",
            new { entity.NombreCompleto, entity.Cedula, entity.CamionPreferidoId, entity.IsActive },
            ct: cancellationToken);

        return ResponseBase<AsistenteDto>.Ok(new AsistenteDto
        {
            Id = entity.Id,
            NombreCompleto = entity.NombreCompleto,
            Cedula = entity.Cedula,
            Telefono = entity.Telefono,
            CamionPreferidoId = entity.CamionPreferidoId,
            CamionPreferidoNombre = camionNombre,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        }, isNew ? "Asistente creado" : "Asistente actualizado");
    }
}
