using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Planillas.AuthorizeShortage;

public class AuthorizeShortageCommand : IRequest<ResponseBase<PlanillaDto>>
{
    public int Id { get; set; }
    public bool Approve { get; set; }
    public string? Motivo { get; set; }
}
