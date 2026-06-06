using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.Notifications.MarkRead;

public class MarkReadCommand : IRequest<ResponseBase<bool>>
{
    public int? Id { get; set; }
    public bool All { get; set; } = false;
}
