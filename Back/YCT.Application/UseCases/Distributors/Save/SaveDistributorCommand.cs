using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Distributors.Save;

public record SaveDistributorCommand(
    int? Id,
    string Name,
    string? Phone,
    string VehicleType,
    string? VehiclePlate,
    string? Notes,
    bool IsActive) : IRequest<ResponseBase<DistributorDto>>;
