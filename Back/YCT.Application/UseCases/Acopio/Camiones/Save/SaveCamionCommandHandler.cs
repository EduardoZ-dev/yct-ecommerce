using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Camiones.Save;

public class SaveCamionCommandHandler : IRequestHandler<SaveCamionCommand, ResponseBase<CamionDto>>
{
    private static readonly string[] EstadosValidos = { "Activo", "Mantenimiento", "Inactivo" };

    private readonly IGenericRepository<Camion> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public SaveCamionCommandHandler(
        IGenericRepository<Camion> repository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<CamionDto>> Handle(SaveCamionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return ResponseBase<CamionDto>.Fail("El nombre del camión es obligatorio");

        var estado = EstadosValidos.Contains(request.Estado) ? request.Estado : "Activo";
        var nombre = request.Nombre.Trim();

        var existentes = await _repository.FindAsync(c => c.Nombre == nombre);
        var duplicado = existentes.FirstOrDefault();
        if (duplicado != null && duplicado.Id != (request.Id ?? 0))
            return ResponseBase<CamionDto>.Fail($"Ya existe un camión llamado '{nombre}'");

        Camion entity;
        bool isNew = !request.Id.HasValue || request.Id == 0;

        if (isNew)
        {
            entity = new Camion
            {
                Nombre = nombre,
                Placa = request.Placa?.Trim(),
                Estado = estado,
                Notas = request.Notas?.Trim()
            };
            await _repository.AddAsync(entity);
        }
        else
        {
            var existing = await _repository.GetByIdAsync(request.Id!.Value);
            if (existing == null)
                return ResponseBase<CamionDto>.Fail("Camión no encontrado");

            existing.Nombre = nombre;
            existing.Placa = request.Placa?.Trim();
            existing.Estado = estado;
            existing.Notas = request.Notas?.Trim();
            existing.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(existing);
            entity = existing;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(isNew ? "Create" : "Update", "Camion", entity.Id,
            $"Camión {(isNew ? "creado" : "actualizado")}: {entity.Nombre} ({entity.Estado})",
            new { entity.Nombre, entity.Placa, entity.Estado },
            ct: cancellationToken);

        return ResponseBase<CamionDto>.Ok(new CamionDto
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Placa = entity.Placa,
            Estado = entity.Estado,
            Notas = entity.Notas,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        }, isNew ? "Camión creado" : "Camión actualizado");
    }
}
