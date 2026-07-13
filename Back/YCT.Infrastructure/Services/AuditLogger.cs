using System.Text.Json;
using YCT.Application.Common;
using YCT.Domain.Common;
using YCT.Domain.Entities;
using YCT.Infrastructure.Persistence;

namespace YCT.Infrastructure.Services;

public class AuditLogger : IAuditLogger
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditLogger(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task LogAsync(
        string action,
        string entityType,
        int? entityId,
        string summary,
        object? details = null,
        bool success = true,
        int? overrideUserId = null,
        string? overrideUsername = null,
        CancellationToken ct = default)
    {
        // El JWT del chofer y el de recepción llevan el id de CONDUCTOR (acopio.Conductores),
        // no el de un usuario del panel (dbo.Users). Guardarlo como UserId rompía la FK
        // FK_AuditLogs_Users_UserId y tumbaba TODA la transacción: la planilla no se guardaba
        // y la tablet la reintentaba para siempre ("pendiente" pegado). Y cuando el id sí
        // existía por casualidad (conductores 1-3), la auditoría quedaba atribuida a otra
        // persona. Para esos roles no hay usuario del panel: UserId va null y la trazabilidad
        // se conserva en Username + UserRole.
        var log = new AuditLog
        {
            UserId = overrideUserId ?? (EsUsuarioDelPanel(_currentUser.Role) ? _currentUser.UserId : null),
            // El token del chofer no trae el claim Name (solo GivenName), así que sin el
            // FullName de respaldo TODA su auditoría quedaba como "anonymous": imposible
            // saber qué chofer hizo qué.
            Username = overrideUsername ?? _currentUser.Username ?? _currentUser.FullName ?? "anonymous",
            UserRole = _currentUser.Role ?? "Anonymous",
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Summary = summary[..Math.Min(summary.Length, 500)],
            Details = details is null ? null : JsonSerializer.Serialize(details, JsonOpts),
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent,
            Success = success,
            CreatedAt = DateTime.UtcNow
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// ¿El id que trae el token corresponde de verdad a una fila de dbo.Users?
    ///
    /// SÍ para todos los roles del panel y también para Recepción: esa tablet se autentica
    /// contra dbo.Users (ver RecepcionLoginCommandHandler), así que su id es legítimo.
    /// NO para Conductor: la app del chofer se autentica contra acopio.Conductores, y su id
    /// no tiene nada que ver con dbo.Users.
    /// </summary>
    private static bool EsUsuarioDelPanel(string? rol) => rol != Roles.Conductor;
}
