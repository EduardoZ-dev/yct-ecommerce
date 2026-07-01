using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Common;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Recepcion.Login;

public class RecepcionLoginCommandHandler : IRequestHandler<RecepcionLoginCommand, ResponseBase<RecepcionLoginResponse>>
{
    private readonly IGenericRepository<User> _users;
    private readonly IConfiguration _config;
    private readonly IAuditLogger _audit;

    public RecepcionLoginCommandHandler(IGenericRepository<User> users, IConfiguration config, IAuditLogger audit)
    {
        _users = users;
        _config = config;
        _audit = audit;
    }

    public async Task<ResponseBase<RecepcionLoginResponse>> Handle(RecepcionLoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = request.Usuario?.Trim() ?? "";
        var clave = request.Clave ?? "";
        if (usuario.Length == 0 || clave.Length == 0)
            return ResponseBase<RecepcionLoginResponse>.Fail("Usuario y clave son obligatorios");

        var user = (await _users.FindAsync(u => u.Username == usuario && u.IsActive)).FirstOrDefault();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(clave))).ToLowerInvariant();

        // Solo usuarios con rol Recepcion pueden entrar por esta puerta.
        if (user == null || user.Role != Roles.Recepcion || user.PasswordHash != hash)
        {
            await _audit.LogAsync("RecepcionLoginFailed", "Auth", user?.Id,
                $"Login recepción fallido para usuario {usuario}",
                new { usuario }, success: false, ct: cancellationToken);
            return ResponseBase<RecepcionLoginResponse>.Fail("Usuario o clave incorrectos");
        }

        var token = GenerateToken(user);

        await _audit.LogAsync("RecepcionLogin", "Auth", user.Id,
            $"{user.FullName} (Recepción) inició sesión en la tablet",
            new { user.Username }, ct: cancellationToken);

        return ResponseBase<RecepcionLoginResponse>.Ok(new RecepcionLoginResponse
        {
            Token = token,
            Username = user.Username,
            FullName = user.FullName
        });
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.FullName),
            new(ClaimTypes.Role, Roles.Recepcion),
        };

        // Token largo: la tablet de recepción vive fija en planta.
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
