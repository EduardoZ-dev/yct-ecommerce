namespace YCT.Application.Common;

/// <summary>
/// Acceso al usuario autenticado del request HTTP actual.
/// Útil para handlers que necesitan saber quién hizo la acción.
/// </summary>
public interface ICurrentUser
{
    int? UserId { get; }
    string? Username { get; }
    string? FullName { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
