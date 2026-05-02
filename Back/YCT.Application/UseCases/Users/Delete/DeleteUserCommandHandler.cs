using MediatR;
using YCT.Application.Common;
using YCT.Domain.Common;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Users.Delete;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public DeleteUserCommandHandler(
        IGenericRepository<User> repository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<ResponseBase<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id);
        if (user == null)
            return ResponseBase<bool>.Fail("Usuario no encontrado");

        if (_currentUser.UserId == user.Id)
            return ResponseBase<bool>.Fail("No puedes eliminar tu propia cuenta");

        if (user.Role == Roles.SuperAdmin && _currentUser.Role != Roles.SuperAdmin)
            return ResponseBase<bool>.Fail("Solo un SuperAdmin puede eliminar otro SuperAdmin");

        // Soft delete: solo desactivamos. Mantiene integridad referencial con Orders/AuditLogs.
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Delete", "User", user.Id,
            $"Usuario desactivado: {user.Username}",
            new { user.Username, user.Role },
            ct: cancellationToken);

        return ResponseBase<bool>.Ok(true, "Usuario desactivado");
    }
}
