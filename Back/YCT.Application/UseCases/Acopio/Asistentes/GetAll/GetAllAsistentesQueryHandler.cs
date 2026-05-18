using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Asistentes.GetAll;

public class GetAllAsistentesQueryHandler : IRequestHandler<GetAllAsistentesQuery, ResponseBase<List<AsistenteDto>>>
{
    private readonly IGenericRepository<Asistente> _repository;

    public GetAllAsistentesQueryHandler(IGenericRepository<Asistente> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<List<AsistenteDto>>> Handle(GetAllAsistentesQuery request, CancellationToken cancellationToken)
    {
        var all = (await _repository.GetAllAsync(a => a.CamionPreferido!)).ToList();
        var dtos = all
            .OrderByDescending(a => a.IsActive)
            .ThenBy(a => a.NombreCompleto)
            .Select(a => new AsistenteDto
            {
                Id = a.Id,
                NombreCompleto = a.NombreCompleto,
                Cedula = a.Cedula,
                Telefono = a.Telefono,
                CamionPreferidoId = a.CamionPreferidoId,
                CamionPreferidoNombre = a.CamionPreferido?.Nombre,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList();

        return ResponseBase<List<AsistenteDto>>.Ok(dtos);
    }
}
