using System.Security.Cryptography;
using System.Text;
using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Common;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Users.Create;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ResponseBase<UserDto>>
{
    private readonly IGenericRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public CreateUserCommandHandler(
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

    public async Task<ResponseBase<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!Roles.IsValid(request.Role))
            return ResponseBase<UserDto>.Fail($"Rol inválido. Válidos: {string.Join(", ", Roles.All)}");

        // Solo SuperAdmin puede crear otros SuperAdmin
        if (request.Role == Roles.SuperAdmin && _currentUser.Role != Roles.SuperAdmin)
            return ResponseBase<UserDto>.Fail("Solo un SuperAdmin puede crear otro SuperAdmin");

        var existing = await _repository.FindAsync(u => u.Username == request.Username);
        if (existing.Any())
            return ResponseBase<UserDto>.Fail("Ya existe un usuario con ese nombre");

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password))).ToLower();

        var user = new User
        {
            Username = request.Username,
            PasswordHash = hash,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Role = request.Role,
            IsActive = true
        };

        await _repository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Create", "User", user.Id,
            $"Usuario creado: {user.Username} ({user.Role})",
            new { user.Username, user.Role, user.FullName, user.Email },
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
        }, "Usuario creado exitosamente");
    }
}
