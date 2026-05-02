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
            if (!string.IsNullOrWhiteSpace(fwd)) return fwd.Split(',').First().Trim();
            return ctx.Connection.RemoteIpAddress?.ToString();
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
