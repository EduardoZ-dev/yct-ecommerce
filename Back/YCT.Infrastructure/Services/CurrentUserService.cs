using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using YCT.Application.Common;

namespace YCT.Infrastructure.Services;

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
    {
        _http = http;
    }

    private ClaimsPrincipal? Principal => _http.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public int? UserId
    {
        get
        {
            var raw = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? Username => Principal?.FindFirstValue(ClaimTypes.Name);
    public string? FullName => Principal?.FindFirstValue(ClaimTypes.GivenName);
    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

    public string? IpAddress
    {
        get
        {
            var ctx = _http.HttpContext;
            if (ctx == null) return null;
            // Respeta forwarded headers si existen
            var fwd = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            var ip = !string.IsNullOrWhiteSpace(fwd)
                ? fwd.Split(',').First().Trim()
                : ctx.Connection.RemoteIpAddress?.ToString();
            // Se TRUNCA a 45 (el largo de AuditLog.IpAddress). Un X-Forwarded-For largo
            // reventaba el SaveChanges de la auditoría DESPUÉS de commitear el dato de
            // negocio: 500 con el dato ya guardado → la tablet reintentaba para siempre.
            // Es el mismo mecanismo del bug de la FK de UserId, por otra columna.
            return string.IsNullOrWhiteSpace(ip) ? null : ip[..Math.Min(ip.Length, 45)];
        }
    }

    public string? UserAgent
    {
        get
        {
            var raw = _http.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();
            return string.IsNullOrWhiteSpace(raw) ? null : raw[..Math.Min(raw.Length, 300)];
        }
    }
}
