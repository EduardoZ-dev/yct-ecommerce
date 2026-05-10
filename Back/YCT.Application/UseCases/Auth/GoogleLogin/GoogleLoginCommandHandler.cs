using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Common;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Auth.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, ResponseBase<LoginResponse>>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _audit;

    public GoogleLoginCommandHandler(
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IAuditLogger audit)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _audit = audit;
    }

    public async Task<ResponseBase<LoginResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var clientId = _configuration["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            return ResponseBase<LoginResponse>.Fail("Google sign-in no está configurado en el servidor");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });
        }
        catch (InvalidJwtException)
        {
            await _audit.LogAsync("GoogleLoginFailed", "Auth", null,
                "Token de Google inválido", new { reason = "invalid_jwt" },
                success: false, ct: cancellationToken);
            return ResponseBase<LoginResponse>.Fail("Token de Google inválido");
        }

        if (!payload.EmailVerified)
            return ResponseBase<LoginResponse>.Fail("El email de Google no está verificado");

        var byGoogle = await _userRepository.FindAsync(u => u.GoogleId == payload.Subject);
        var user = byGoogle.FirstOrDefault();

        if (user == null)
        {
            var byEmail = await _userRepository.FindAsync(u => u.Email == payload.Email);
            user = byEmail.FirstOrDefault();
        }

        if (user == null || user.Role == Roles.Customer)
        {
            await _audit.LogAsync("GoogleLoginFailed", "Auth", user?.Id,
                $"Acceso denegado para {payload.Email}: cuenta sin permisos administrativos",
                new { payload.Email, reason = "not_authorized" },
                success: false,
                overrideUsername: payload.Email,
                ct: cancellationToken);
            return ResponseBase<LoginResponse>.Fail("Esta cuenta de Google no tiene acceso administrativo");
        }

        if (!user.IsActive)
        {
            await _audit.LogAsync("GoogleLoginFailed", "Auth", user.Id,
                $"Cuenta desactivada: {user.Username}",
                new { user.Username, reason = "inactive" },
                success: false, overrideUserId: user.Id, overrideUsername: user.Username,
                ct: cancellationToken);
            return ResponseBase<LoginResponse>.Fail("La cuenta está desactivada");
        }

        if (user.GoogleId != payload.Subject || string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            user.GoogleId = payload.Subject;
            if (string.IsNullOrWhiteSpace(user.AvatarUrl)) user.AvatarUrl = payload.Picture;
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var token = GenerateJwtToken(user);

        await _audit.LogAsync("Login", "Auth", user.Id,
            $"{user.Username} ({user.Role}) inició sesión con Google",
            new { user.Username, user.Role, provider = "google" },
            overrideUserId: user.Id, overrideUsername: user.Username,
            ct: cancellationToken);

        return ResponseBase<LoginResponse>.Ok(new LoginResponse
        {
            Token = token,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl
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
