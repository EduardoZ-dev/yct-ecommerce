using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YCT.Application.Common;
using YCT.Infrastructure.Persistence;

namespace YCT.Infrastructure.Services;

/// <summary>
/// Envía reporte diario a admin con resumen de planillas del día.
/// Ejecuta una vez por día a las 22:00 hora local del servidor.
/// </summary>
public class DailyReportService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DailyReportService> _logger;
    private const int RunHour = 22; // 10pm

    public DailyReportService(IServiceProvider services, ILogger<DailyReportService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var next = now.Date.AddHours(RunHour);
            if (next <= now) next = next.AddDays(1);
            var delay = next - now;
            _logger.LogInformation("DailyReport scheduled at {Next} (in {Hours:F1}h)", next, delay.TotalHours);

            try { await Task.Delay(delay, stoppingToken); }
            catch (TaskCanceledException) { break; }

            try { await SendDailyReport(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "DailyReport failed"); }
        }
    }

    private async Task SendDailyReport(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var today = DateTime.Today;
        var rutas = await db.Rutas
            .Include(r => r.Camion)
            .Include(r => r.Conductor)
            .Where(r => r.Fecha.Date == today)
            .ToListAsync(ct);

        if (rutas.Count == 0)
        {
            _logger.LogInformation("No hay rutas hoy, no se envía reporte");
            return;
        }

        var conciliadas = rutas.Where(r => r.Status == "Conciliada").ToList();
        var pendientes = rutas.Where(r => r.Status == "PendienteAutorizacion").ToList();
        var enProgreso = rutas.Where(r => r.Status == "EnProgreso" || r.Status == "EsperandoDescargue").ToList();
        var anuladas = rutas.Where(r => r.Status == "Anulada").ToList();

        var totalChofer = rutas.Sum(r => r.TotalLitrosChofer);
        var totalPlanta = rutas.Sum(r => r.TotalLitrosPlanta ?? 0);

        var html = $@"
            <h2>Reporte diario de acopio · {today:dd/MM/yyyy}</h2>
            <h3>Resumen</h3>
            <ul>
              <li><b>Rutas totales:</b> {rutas.Count}</li>
              <li><b>Conciliadas:</b> {conciliadas.Count}</li>
              <li><b>Pendientes autorización:</b> <span style='color:#e53935'>{pendientes.Count}</span></li>
              <li><b>En progreso / esperando descargue:</b> {enProgreso.Count}</li>
              <li><b>Anuladas:</b> {anuladas.Count}</li>
              <li><b>Total declarado por choferes:</b> {totalChofer:F2} L</li>
              <li><b>Total descargado en planta:</b> {totalPlanta:F2} L</li>
              <li><b>Diferencia neta:</b> {(totalPlanta - totalChofer):F2} L</li>
            </ul>";

        if (pendientes.Any())
        {
            html += "<h3 style='color:#e53935'>⚠️ Planillas pendientes de autorización</h3><ul>";
            foreach (var r in pendientes)
            {
                html += $"<li>{r.Codigo} · {r.Conductor?.NombreCompleto} · Chofer: {r.TotalLitrosChofer:F2} L · Planta: {r.TotalLitrosPlanta:F2} L · <b>Falta: {Math.Abs(r.DiferenciaTotal ?? 0):F2} L</b></li>";
            }
            html += "</ul>";
        }

        html += "<h3>Detalle por ruta</h3><table border='1' cellpadding='6' style='border-collapse:collapse;font-family:Arial;font-size:13px'>";
        html += "<tr style='background:#5A9E2F;color:white'><th>Ruta</th><th>Conductor</th><th>Chofer (L)</th><th>Planta (L)</th><th>Diff</th><th>Estado</th></tr>";
        foreach (var r in rutas.OrderBy(x => x.Codigo))
        {
            var diffStr = r.DiferenciaTotal.HasValue ? $"{r.DiferenciaTotal:F2}" : "—";
            html += $"<tr><td>{r.Codigo}</td><td>{r.Conductor?.NombreCompleto}</td><td align='right'>{r.TotalLitrosChofer:F2}</td><td align='right'>{r.TotalLitrosPlanta?.ToString("F2") ?? "—"}</td><td align='right'>{diffStr}</td><td>{r.Status}</td></tr>";
        }
        html += "</table>";

        try
        {
            await email.SendAsync("yairevarduardozeq@gmail.com",
                $"📊 Reporte diario acopio · {today:dd/MM/yyyy}",
                html, null, ct);
            _logger.LogInformation("DailyReport sent with {Count} rutas", rutas.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send DailyReport email");
        }
    }
}
