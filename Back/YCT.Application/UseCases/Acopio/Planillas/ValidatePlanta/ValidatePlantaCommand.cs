using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Planillas.ValidatePlanta;

public class ValidatePlantaCommand : IRequest<ResponseBase<PlanillaDto>>
{
    public int Id { get; set; }
    public decimal TotalLitrosPlanta { get; set; }
    public int? CantinasPlanta { get; set; }
    public decimal? LitrosSueltosPlanta { get; set; }
    public TimeSpan? HoraDescargue { get; set; }
    public string? Observaciones { get; set; }
}
