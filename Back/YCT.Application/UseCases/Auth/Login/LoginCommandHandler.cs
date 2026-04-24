using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ResponseBase<LoginResponse>>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(IGenericRepository<User> userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<ResponseBase<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.FindAsync(u => u.Username == request.Username && u.IsActive);
        var user = users.FirstOrDefault();

        if (user == null)
            return ResponseBase<LoginResponse>.Fail("Usuario o contraseña incorrectos");

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password))).ToLower();
        if (user.PasswordHash != hash)
            return ResponseBase<LoginResponse>.Fail("Usuario o contraseña incorrectos");

        var token = GenerateJwtToken(user);

        return ResponseBase<LoginResponse>.Ok(new LoginResponse
        {
            Token = token,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role
        });
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.GivenName, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
