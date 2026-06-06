using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Notifications.GetAll;

public class GetAllNotificationsQuery : IRequest<ResponseBase<List<NotificationDto>>>
{
    public bool OnlyUnread { get; set; } = false;
    public int Take { get; set; } = 50;
}
