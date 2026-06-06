using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Notifications.MarkRead;

public class MarkReadCommandHandler : IRequestHandler<MarkReadCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<Notification> _repo;
    private readonly IUnitOfWork _uow;

    public MarkReadCommandHandler(IGenericRepository<Notification> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<ResponseBase<bool>> Handle(MarkReadCommand request, CancellationToken cancellationToken)
    {
        if (request.All)
        {
            var all = await _repo.FindAsync(n => !n.IsRead);
            foreach (var n in all)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
                await _repo.UpdateAsync(n);
            }
        }
        else if (request.Id.HasValue)
        {
            var n = await _repo.GetByIdAsync(request.Id.Value);
            if (n == null) return ResponseBase<bool>.Fail("Notificación no encontrada");
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            await _repo.UpdateAsync(n);
        }
        else
        {
            return ResponseBase<bool>.Fail("Especifica id o all=true");
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return ResponseBase<bool>.Ok(true, "Marcado como leído");
    }
}
