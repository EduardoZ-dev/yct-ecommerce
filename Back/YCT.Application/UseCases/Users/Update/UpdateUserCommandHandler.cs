using System.Security.Cryptography;
using System.Text;
using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Common;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Users.Update;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ResponseBase<UserDto>>
{
    private readonly IGenericRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public UpdateUserCommandHandler(
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

    public async Task<ResponseBase<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (!Roles.IsValid(request.Role))
            return ResponseBase<UserDto>.Fail($"Rol inválido. Válidos: {string.Join(", ", Roles.All)}");

        var user = await _repository.GetByIdAsync(request.Id);
        if (user == null)
            return ResponseBase<UserDto>.Fail("Usuario no encontrado");

        // No puedes degradarte a ti mismo
        if (_currentUser.UserId == user.Id && _currentUser.Role != request.Role)
            return ResponseBase<UserDto>.Fail("No puedes cambiar tu propio rol");

        // Solo SuperAdmin puede asignar/quitar SuperAdmin
        if ((user.Role == Roles.SuperAdmin || request.Role == Roles.SuperAdmin)
            && _currentUser.Role != Roles.SuperAdmin)
            return ResponseBase<UserDto>.Fail("Solo un SuperAdmin puede gestionar usuarios SuperAdmin");

        var before = new { user.FullName, user.Email, user.Phone, user.Role, user.IsActive };

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        var passwordChanged = false;
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            user.PasswordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.NewPassword))).ToLower();
            passwordChanged = true;
        }

        await _repository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Update", "User", user.Id,
            $"Usuario actualizado: {user.Username}{(passwordChanged ? " (contraseña reseteada)" : "")}",
            new
            {
                before,
                after = new { user.FullName, user.Email, user.Phone, user.Role, user.IsActive },
                passwordChanged
            },
            ct: cancellationToken);

        return ResponseBase<UserDto>.Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }, "Usuario actualizado exitosamente");
    }
}
