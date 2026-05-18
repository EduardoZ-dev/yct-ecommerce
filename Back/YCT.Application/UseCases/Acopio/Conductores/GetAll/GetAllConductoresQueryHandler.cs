using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Conductores.GetAll;

public class GetAllConductoresQueryHandler : IRequestHandler<GetAllConductoresQuery, ResponseBase<List<ConductorDto>>>
{
    private readonly IGenericRepository<Conductor> _repository;

    public GetAllConductoresQueryHandler(IGenericRepository<Conductor> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<List<ConductorDto>>> Handle(GetAllConductoresQuery request, CancellationToken cancellationToken)
    {
        var all = (await _repository.GetAllAsync(c => c.CamionPreferido!)).ToList();
        var dtos = all
            .OrderByDescending(c => c.IsActive)
            .ThenBy(c => c.NombreCompleto)
            .Select(c => new ConductorDto
            {
                Id = c.Id,
                NombreCompleto = c.NombreCompleto,
                Cedula = c.Cedula,
                Telefono = c.Telefono,
                CamionPreferidoId = c.CamionPreferidoId,
                CamionPreferidoNombre = c.CamionPreferido?.Nombre,
                UserId = c.UserId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

        return ResponseBase<List<ConductorDto>>.Ok(dtos);
    }
}
