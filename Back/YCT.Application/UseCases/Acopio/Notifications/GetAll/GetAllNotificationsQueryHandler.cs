using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Notifications.GetAll;

public class GetAllNotificationsQueryHandler : IRequestHandler<GetAllNotificationsQuery, ResponseBase<List<NotificationDto>>>
{
    private readonly IGenericRepository<Notification> _repo;

    public GetAllNotificationsQueryHandler(IGenericRepository<Notification> repo)
    {
        _repo = repo;
    }

    public async Task<ResponseBase<List<NotificationDto>>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        var all = await _repo.GetAllAsync();
        var filtered = all
            .Where(n => !request.OnlyUnread || !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(request.Take)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                PlanillaId = n.PlanillaId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToList();
        return ResponseBase<List<NotificationDto>>.Ok(filtered);
    }
}
