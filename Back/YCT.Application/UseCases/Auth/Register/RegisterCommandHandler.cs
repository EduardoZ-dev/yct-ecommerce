using System.Security.Cryptography;
using System.Text;
using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ResponseBase<UserDto>>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(IGenericRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseBase<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.FindAsync(u => u.Username == request.Username);
        if (existing.Any())
            return ResponseBase<UserDto>.Fail("El nombre de usuario ya existe");

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password))).ToLower();

        var user = new User
        {
            Username = request.Username,
            PasswordHash = hash,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Role = "Customer"
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ResponseBase<UserDto>.Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        }, "Usuario registrado exitosamente");
    }
}
