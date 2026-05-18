using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Camiones.GetAll;

public class GetAllCamionesQueryHandler : IRequestHandler<GetAllCamionesQuery, ResponseBase<List<CamionDto>>>
{
    private readonly IGenericRepository<Camion> _repository;

    public GetAllCamionesQueryHandler(IGenericRepository<Camion> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<List<CamionDto>>> Handle(GetAllCamionesQuery request, CancellationToken cancellationToken)
    {
        var all = (await _repository.GetAllAsync()).ToList();
        var dtos = all
            .OrderBy(c => c.Nombre)
            .Select(c => new CamionDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Placa = c.Placa,
                Estado = c.Estado,
                Notas = c.Notas,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

        return ResponseBase<List<CamionDto>>.Ok(dtos);
    }
}
