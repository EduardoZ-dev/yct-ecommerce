using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Granjeros.Save;

public class SaveGranjeroCommandHandler : IRequestHandler<SaveGranjeroCommand, ResponseBase<GranjeroDto>>
{
    private readonly IGenericRepository<Granjero> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public SaveGranjeroCommandHandler(
        IGenericRepository<Granjero> repository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<GranjeroDto>> Handle(SaveGranjeroCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NombreCompleto))
            return ResponseBase<GranjeroDto>.Fail("El nombre completo es obligatorio");
        if (request.Numero <= 0)
            return ResponseBase<GranjeroDto>.Fail("El número del proveedor debe ser mayor a 0");
        if (request.PrecioLitro.HasValue && request.PrecioLitro < 0)
            return ResponseBase<GranjeroDto>.Fail("El precio por litro no puede ser negativo");

        var existentes = await _repository.FindAsync(g => g.Numero == request.Numero);
        var duplicado = existentes.FirstOrDefault();
        if (duplicado != null && duplicado.Id != (request.Id ?? 0))
            return ResponseBase<GranjeroDto>.Fail($"Ya existe un granjero con el número {request.Numero}");

        Granjero entity;
        bool isNew = !request.Id.HasValue || request.Id == 0;

        if (isNew)
        {
            entity = new Granjero
            {
                Numero = request.Numero,
                NombreCompleto = request.NombreCompleto.Trim(),
                Cedula = request.Cedula?.Trim(),
                Telefono = request.Telefono?.Trim(),
                Finca = request.Finca?.Trim(),
                Vereda = request.Vereda?.Trim(),
                Municipio = request.Municipio?.Trim(),
                PrecioLitro = request.PrecioLitro,
                Notas = request.Notas?.Trim(),
                IsActive = request.IsActive
            };
            await _repository.AddAsync(entity);
        }
        else
        {
            var existing = await _repository.GetByIdAsync(request.Id!.Value);
            if (existing == null)
                return ResponseBase<GranjeroDto>.Fail("Granjero no encontrado");

            existing.Numero = request.Numero;
            existing.NombreCompleto = request.NombreCompleto.Trim();
            existing.Cedula = request.Cedula?.Trim();
            existing.Telefono = request.Telefono?.Trim();
            existing.Finca = request.Finca?.Trim();
            existing.Vereda = request.Vereda?.Trim();
            existing.Municipio = request.Municipio?.Trim();
            existing.PrecioLitro = request.PrecioLitro;
            existing.Notas = request.Notas?.Trim();
            existing.IsActive = request.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(existing);
            entity = existing;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(isNew ? "Create" : "Update", "Granjero", entity.Id,
            $"Granjero {(isNew ? "creado" : "actualizado")}: #{entity.Numero} {entity.NombreCompleto}",
            new { entity.Numero, entity.NombreCompleto, entity.PrecioLitro, entity.IsActive },
            ct: cancellationToken);

        return ResponseBase<GranjeroDto>.Ok(new GranjeroDto
        {
            Id = entity.Id,
            Numero = entity.Numero,
            NombreCompleto = entity.NombreCompleto,
            Cedula = entity.Cedula,
            Telefono = entity.Telefono,
            Finca = entity.Finca,
            Vereda = entity.Vereda,
            Municipio = entity.Municipio,
            PrecioLitro = entity.PrecioLitro,
            PromedioDiario = entity.PromedioDiario,
            Notas = entity.Notas,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        }, isNew ? "Granjero creado" : "Granjero actualizado");
    }
}
