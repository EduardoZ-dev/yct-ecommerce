using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using YCT.Application.Common;
using YCT.Domain.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Chofer.Login;

public class ChoferLoginCommandHandler : IRequestHandler<ChoferLoginCommand, ResponseBase<ChoferLoginResponse>>
{
    private readonly IGenericRepository<Conductor> _conductores;
    private readonly IGenericRepository<Camion> _camiones;
    private readonly IConfiguration _config;
    private readonly IAuditLogger _audit;

    public ChoferLoginCommandHandler(
        IGenericRepository<Conductor> conductores,
        IGenericRepository<Camion> camiones,
        IConfiguration config,
        IAuditLogger audit)
    {
        _conductores = conductores;
        _camiones = camiones;
        _config = config;
        _audit = audit;
    }

    public async Task<ResponseBase<ChoferLoginResponse>> Handle(ChoferLoginCommand request, CancellationToken cancellationToken)
    {
        var cedula = request.Cedula?.Trim() ?? "";
        var pin = request.Pin?.Trim() ?? "";
        if (cedula.Length == 0 || pin.Length == 0)
            return ResponseBase<ChoferLoginResponse>.Fail("Cédula y PIN son obligatorios");

        var conductor = (await _conductores.FindAsync(c => c.Cedula == cedula && c.IsActive)).FirstOrDefault();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pin))).ToLowerInvariant();

        if (conductor == null || string.IsNullOrEmpty(conductor.PinHash) || conductor.PinHash != hash)
        {
            await _audit.LogAsync("ChoferLoginFailed", "Auth", conductor?.Id,
                $"Login chofer fallido para cédula {cedula}",
                new { cedula }, success: false, ct: cancellationToken);
            return ResponseBase<ChoferLoginResponse>.Fail("Cédula o PIN incorrectos");
        }

        string? camionNombre = null;
        if (conductor.CamionPreferidoId.HasValue)
            camionNombre = (await _camiones.GetByIdAsync(conductor.CamionPreferidoId.Value))?.Nombre;

        var token = GenerateToken(conductor);

        await _audit.LogAsync("ChoferLogin", "Auth", conductor.Id,
            $"{conductor.NombreCompleto} (Conductor) inició sesión en la app",
            new { conductor.NombreCompleto, conductor.Cedula }, ct: cancellationToken);

        return ResponseBase<ChoferLoginResponse>.Ok(new ChoferLoginResponse
        {
            Token = token,
            ConductorId = conductor.Id,
            NombreCompleto = conductor.NombreCompleto,
            CamionPreferidoId = conductor.CamionPreferidoId,
            CamionPreferidoNombre = camionNombre
        });
    }

    private string GenerateToken(Conductor c)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, c.Id.ToString()),
            new(ClaimTypes.GivenName, c.NombreCompleto),
            new(ClaimTypes.Role, Roles.Conductor),
        };
        if (c.CamionPreferidoId.HasValue)
            claims.Add(new Claim("camionPreferidoId", c.CamionPreferidoId.Value.ToString()));

        // Token largo: la app del chofer trabaja offline en ruta y sincroniza luego.
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
