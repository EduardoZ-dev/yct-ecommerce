using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.Save;

public class SaveClienteTerceroCommandHandler : IRequestHandler<SaveClienteTerceroCommand, ResponseBase<ClienteTerceroDto>>
{
    private readonly IGenericRepository<ClienteTercero> _repository;
    private readonly IGenericRepository<EntregaTercero> _entregaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public SaveClienteTerceroCommandHandler(
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

    public async Task<ResponseBase<ClienteTerceroDto>> Handle(SaveClienteTerceroCommand request, CancellationToken cancellationToken)
    {
        var error = await ValidarAsync(request);
        if (error != null)
            return ResponseBase<ClienteTerceroDto>.Fail(error);

        bool isNew = !request.Id.HasValue || request.Id == 0;
        var entity = isNew ? new ClienteTercero() : await _repository.GetByIdAsync(request.Id!.Value);
        if (entity == null)
            return ResponseBase<ClienteTerceroDto>.Fail("Cliente tercero no encontrado");

        Aplicar(entity, request);
        if (isNew)
        {
            await _repository.AddAsync(entity);
        }
        else
        {
            entity.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(entity);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(isNew ? "Create" : "Update", "ClienteTercero", entity.Id,
            $"Cliente tercero {(isNew ? "creado" : "actualizado")}: {entity.NombreCompleto}",
            new { entity.NombreCompleto, entity.Cedula, entity.PrecioLitro, entity.IsActive },
            ct: cancellationToken);

        // Al editar se devuelven los totales reales para que el panel pueda reemplazar la fila sin recargar.
        IEnumerable<EntregaTercero> entregas = isNew
            ? Enumerable.Empty<EntregaTercero>()
            : await _entregaRepository.FindAsync(e => e.ClienteTerceroId == entity.Id);

        return ResponseBase<ClienteTerceroDto>.Ok(
            ClientesTercerosMapper.ToDto(entity, entregas),
            isNew ? "Cliente tercero creado" : "Cliente tercero actualizado");
    }

    /// <summary>Devuelve el mensaje de error o null si el comando es válido.</summary>
    private async Task<string?> ValidarAsync(SaveClienteTerceroCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreCompleto))
            return "El nombre completo es obligatorio";
        if (request.PrecioLitro.HasValue && request.PrecioLitro < 0)
            return "El precio por litro no puede ser negativo";

        var cedula = Limpiar(request.Cedula);
        if (cedula == null)
            return null;

        // La cédula identifica al tercero (no tiene número de cuaderno), por eso no puede repetirse.
        var conMismaCedula = await _repository.FindAsync(c => c.Cedula == cedula);
        var otro = conMismaCedula.FirstOrDefault(c => c.Id != (request.Id ?? 0));
        return otro != null ? $"Ya existe un cliente tercero con la cédula {cedula}" : null;
    }

    private static void Aplicar(ClienteTercero entity, SaveClienteTerceroCommand request)
    {
        entity.NombreCompleto = request.NombreCompleto.Trim();
        entity.Cedula = Limpiar(request.Cedula);
        entity.Telefono = Limpiar(request.Telefono);
        entity.Municipio = Limpiar(request.Municipio);
        entity.PrecioLitro = request.PrecioLitro;
        entity.Notas = Limpiar(request.Notas);
        entity.IsActive = request.IsActive;
    }

    /// <summary>Recorta y convierte vacíos en null: así el índice único de cédula no choca con "".</summary>
    private static string? Limpiar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
