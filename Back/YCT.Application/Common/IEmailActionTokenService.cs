namespace YCT.Application.Common;

public interface IEmailActionTokenService
{
    /// <summary>Genera token firmado con TTL para acción rápida desde email.</summary>
    string Protect(string action, int resourceId, TimeSpan ttl);

    /// <summary>Devuelve (action, resourceId) si el token es válido y no expiró.</summary>
    bool TryUnprotect(string token, out string action, out int resourceId);
}
