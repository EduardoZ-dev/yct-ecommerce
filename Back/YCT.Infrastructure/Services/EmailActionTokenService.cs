using Microsoft.AspNetCore.DataProtection;
using YCT.Application.Common;

namespace YCT.Infrastructure.Services;

public class EmailActionTokenService : IEmailActionTokenService
{
    private readonly ITimeLimitedDataProtector _protector;

    public EmailActionTokenService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("YCT.EmailActions").ToTimeLimitedDataProtector();
    }

    public string Protect(string action, int resourceId, TimeSpan ttl)
    {
        var payload = $"{action}|{resourceId}";
        return _protector.Protect(payload, DateTimeOffset.UtcNow.Add(ttl));
    }

    public bool TryUnprotect(string token, out string action, out int resourceId)
    {
        action = string.Empty;
        resourceId = 0;
        try
        {
            var raw = _protector.Unprotect(token);
            var parts = raw.Split('|');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[1], out var id)) return false;
            action = parts[0];
            resourceId = id;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
