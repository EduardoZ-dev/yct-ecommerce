using MediatR;
using Microsoft.Extensions.Configuration;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Application.UseCases.Acopio.Planillas.GetById;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Planillas.ValidatePlanta;

public class ValidatePlantaCommandHandler : IRequestHandler<ValidatePlantaCommand, ResponseBase<PlanillaDto>>
{
    private readonly IGenericRepository<Ruta> _rutaRepository;
    private readonly IGenericRepository<Notification> _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;
    private readonly IMediator _mediator;
    private readonly IEmailSender _emailSender;
    private readonly IEmailActionTokenService _tokens;
    private readonly IConfiguration _config;
    private readonly IWhatsAppNotifier _whatsApp;
    private readonly IGenericRepository<Conductor> _conductorRepository;
    private readonly IGenericRepository<Camion> _camionRepository;

    public ValidatePlantaCommandHandler(
        IGenericRepository<Ruta> rutaRepository,
        IGenericRepository<Notification> notificationRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit,
        IMediator mediator,
        IEmailSender emailSender,
        IEmailActionTokenService tokens,
        IConfiguration config,
        IWhatsAppNotifier whatsApp,
        IGenericRepository<Conductor> conductorRepository,
        IGenericRepository<Camion> camionRepository)
    {
        _rutaRepository = rutaRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _mediator = mediator;
        _emailSender = emailSender;
        _tokens = tokens;
        _config = config;
        _whatsApp = whatsApp;
        _conductorRepository = conductorRepository;
        _camionRepository = camionRepository;
    }

    public async Task<ResponseBase<PlanillaDto>> Handle(ValidatePlantaCommand request, CancellationToken cancellationToken)
    {
        if (request.TotalLitrosPlanta < 0)
            return ResponseBase<PlanillaDto>.Fail("El total de litros descargados no puede ser negativo");
        if (string.IsNullOrWhiteSpace(request.Observaciones) || request.Observaciones.Trim().Length < 5)
            return ResponseBase<PlanillaDto>.Fail("Las observaciones son obligatorias (mínimo 5 caracteres)");

        var ruta = await _rutaRepository.GetByIdAsync(request.Id);
        if (ruta == null) return ResponseBase<PlanillaDto>.Fail("Planilla no encontrada");

        if (ruta.Status == "Conciliada" || ruta.Status == "Anulada")
            return ResponseBase<PlanillaDto>.Fail($"Planilla en estado {ruta.Status}, no se puede revalidar");

        // Cálculo diferencia: planta - chofer
        var diferencia = request.TotalLitrosPlanta - ruta.TotalLitrosChofer;

        ruta.TotalLitrosPlanta = request.TotalLitrosPlanta;
        ruta.DiferenciaTotal = diferencia;
        if (request.HoraDescargue.HasValue) ruta.HoraDescargue = request.HoraDescargue;
        if (!string.IsNullOrWhiteSpace(request.Observaciones))
            ruta.Observaciones = string.IsNullOrWhiteSpace(ruta.Observaciones)
                ? request.Observaciones
                : $"{ruta.Observaciones}\n[Planta] {request.Observaciones}";

        // Regla negocio YCT: nunca debe faltar leche, siempre sobrar.
        // Si diferencia < 0 (faltó) → PendienteAutorizacion + notificación
        // Si diferencia >= 0 → Conciliada
        bool shortage = diferencia < 0;
        ruta.Status = shortage ? "PendienteAutorizacion" : "Conciliada";
        ruta.UpdatedAt = DateTime.UtcNow;

        await _rutaRepository.UpdateAsync(ruta);

        // Notificación si hay faltante
        if (shortage)
        {
            var notif = new Notification
            {
                Type = "ShortageDetected",
                Title = $"Faltante de leche · Planilla {ruta.Codigo}",
                Message = $"Chofer declaró {ruta.TotalLitrosChofer:F2} L · Planta midió {request.TotalLitrosPlanta:F2} L · Faltan {Math.Abs(diferencia):F2} L. Requiere autorización.",
                UserId = null, // broadcast admins
                PlanillaId = ruta.Id,
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("ValidatePlanta", "Planilla", ruta.Id,
            $"Validación planta: chofer={ruta.TotalLitrosChofer} L, planta={request.TotalLitrosPlanta} L, diff={diferencia} L, status={ruta.Status}",
            new { ruta.Codigo, chofer = ruta.TotalLitrosChofer, planta = request.TotalLitrosPlanta, diferencia, status = ruta.Status },
            ct: cancellationToken);

        // Email admin (best-effort, no bloquea)
        try
        {
            var apiBase = _config["AppUrls:Api"] ?? "http://localhost:5088";
            var adminBase = _config["AppUrls:Admin"] ?? "http://localhost:4300";
            var (subject, html) = shortage
                ? BuildShortageEmail(ruta, request.TotalLitrosPlanta, diferencia,
                    quickAuthUrl: $"{apiBase}/api/acopio/Planillas/quick-authorize?token={Uri.EscapeDataString(_tokens.Protect("authorize-shortage", ruta.Id, TimeSpan.FromHours(48)))}",
                    reviewUrl:    $"{adminBase}/autorizaciones?id={ruta.Id}")
                : BuildConciliadaEmail(ruta, request.TotalLitrosPlanta, diferencia,
                    reviewUrl: $"{adminBase}/planillas?estado=Conciliada");
            await _emailSender.SendAsync("yairevarduardozeq@gmail.com", subject, html, null);
        }
        catch { /* email error no debe romper validación */ }

        // WhatsApp a los contactos (best-effort, no bloquea)
        try
        {
            var conductor = await _conductorRepository.GetByIdAsync(ruta.ConductorId);
            var camion = await _camionRepository.GetByIdAsync(ruta.CamionId);
            var adminBase = _config["AppUrls:Admin"] ?? "http://localhost:4300";
            await _whatsApp.SendDescargueAsync(new WhatsAppDescargueModel(
                Resultado: shortage ? "CON FALTANTE" : "OK",
                Codigo: ruta.Codigo,
                Fecha: ruta.Fecha,
                Conductor: conductor?.NombreCompleto ?? $"#{ruta.ConductorId}",
                Camion: camion?.Nombre ?? $"#{ruta.CamionId}",
                LitrosChofer: ruta.TotalLitrosChofer,
                LitrosPlanta: request.TotalLitrosPlanta,
                Diferencia: diferencia,
                Estado: ruta.Status,
                HistorialUrl: $"{adminBase}/descargues"), cancellationToken);
        }
        catch { /* whatsapp error no debe romper validación */ }

        var result = await _mediator.Send(new GetPlanillaByIdQuery(ruta.Id), cancellationToken);
        return ResponseBase<PlanillaDto>.Ok(result.Data!,
            shortage ? "Planilla con faltante: requiere autorización" : "Planilla conciliada");
    }

    private static (string subject, string html) BuildConciliadaEmail(Ruta ruta, decimal totalPlanta, decimal diff, string reviewUrl)
    {
        var sobra = diff > 0;
        var sobraNote = sobra
            ? $"Sobraron <b>{diff:F2} L</b> respecto a lo declarado por el chofer."
            : $"Coincidencia exacta entre chofer y planta.";
        var subject = $"YCT Acopio · Descargue OK · {ruta.Codigo}";
        var html = EmailShell(
            accent: "#5A9E2F",
            accentDark: "#3F7222",
            bgSoft: "#EDF7E5",
            badgeText: "✓ DESCARGUE CONCILIADO",
            title: $"Planilla {ruta.Codigo}",
            subtitle: $"Fecha {ruta.Fecha:dd/MM/yyyy} · Conductor {ruta.ConductorId}",
            bodyHtml:
                $"<p style='margin:0 0 18px;font-size:15px;color:#1f2a1a;line-height:1.55;'>" +
                $"Todo salió bien. La planilla quedó <b>conciliada</b> automáticamente. {sobraNote}" +
                $"</p>" +
                StatGrid(
                    ("Chofer declaró", $"{ruta.TotalLitrosChofer:F2} L", "#5A9E2F"),
                    ("Planta midió",   $"{totalPlanta:F2} L",            "#3F7222"),
                    ("Diferencia",     $"+{diff:F2} L",                  "#2C8C0F")
                ) +
                $"<div style='margin-top:22px;padding:14px 16px;background:#EDF7E5;border-left:4px solid #5A9E2F;border-radius:8px;font-size:13px;color:#2c3e1f;'>" +
                $"<b>Estado final:</b> Conciliada · sin acción requerida." +
                $"</div>" +
                CtaBar(("Ver planilla", reviewUrl, "#5A9E2F", false))
        );
        return (subject, html);
    }

    private static (string subject, string html) BuildShortageEmail(Ruta ruta, decimal totalPlanta, decimal diff, string quickAuthUrl, string reviewUrl)
    {
        var falta = Math.Abs(diff);
        var subject = $"⚠️ YCT Acopio · Faltante {falta:F2} L · {ruta.Codigo}";
        var html = EmailShell(
            accent: "#D32F2F",
            accentDark: "#8B1A1A",
            bgSoft: "#FDECEA",
            badgeText: "⚠ FALTANTE DETECTADO",
            title: $"Planilla {ruta.Codigo}",
            subtitle: $"Fecha {ruta.Fecha:dd/MM/yyyy} · Conductor {ruta.ConductorId}",
            bodyHtml:
                $"<p style='margin:0 0 18px;font-size:15px;color:#1f2a1a;line-height:1.55;'>" +
                $"La planta midió <b>menos leche</b> de la declarada por el chofer. Se registró como " +
                $"<b style='color:#D32F2F;'>Pendiente de autorización</b>." +
                $"</p>" +
                StatGrid(
                    ("Chofer declaró", $"{ruta.TotalLitrosChofer:F2} L", "#5A9E2F"),
                    ("Planta midió",   $"{totalPlanta:F2} L",            "#D32F2F"),
                    ("Faltan",         $"−{falta:F2} L",                 "#B71C1C")
                ) +
                $"<div style='margin-top:22px;padding:16px;background:#FDECEA;border:1px solid #F5C4C0;border-left:4px solid #D32F2F;border-radius:10px;'>" +
                $"<div style='font-size:13px;font-weight:700;color:#8B1A1A;margin-bottom:6px;text-transform:uppercase;letter-spacing:.5px;'>Acción requerida</div>" +
                $"<div style='font-size:14px;color:#2c1c1c;line-height:1.5;'>" +
                $"Ingresa a <b>YCT Acopio · Autorizaciones</b> y registra el motivo del faltante o rechaza la planilla." +
                $"</div></div>" +
                CtaBar(
                    ("Autorizar ahora", quickAuthUrl, "#D32F2F", true),
                    ("Ir a revisarlo", reviewUrl, "#5A9E2F", false)
                ) +
                $"<p style='margin-top:14px;font-size:11px;color:#7a8470;text-align:center;'>El enlace «Autorizar ahora» expira en 48 horas.</p>"
        );
        return (subject, html);
    }

    private static string CtaBar(params (string label, string url, string color, bool primary)[] btns)
    {
        var cells = string.Join("", btns.Select(b =>
        {
            var bg = b.primary ? b.color : "#ffffff";
            var fg = b.primary ? "#ffffff" : b.color;
            var border = b.primary ? "transparent" : b.color;
            return
                $"<td style='padding:0 6px;'>" +
                $"<a href='{b.url}' style='display:inline-block;padding:12px 22px;background:{bg};color:{fg};border:2px solid {border};border-radius:10px;text-decoration:none;font-weight:700;font-size:14px;letter-spacing:.3px;'>" +
                $"{b.label}" +
                $"</a></td>";
        }));
        return $"<table cellpadding='0' cellspacing='0' style='margin:22px auto 4px;'><tr>{cells}</tr></table>";
    }

    private static string EmailShell(
        string accent, string accentDark, string bgSoft,
        string badgeText, string title, string subtitle, string bodyHtml)
    {
        return
            $"<div style='background:#f4f6f1;padding:24px;font-family:Segoe UI,Roboto,Arial,sans-serif;'>" +
            $"  <div style='max-width:560px;margin:0 auto;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 6px 24px rgba(30,60,20,.12);border:1px solid #e5ecdc;'>" +
            $"    <div style='background:linear-gradient(135deg,{accent} 0%,{accentDark} 100%);padding:24px 28px;color:#fff;'>" +
            $"      <div style='font-size:11px;letter-spacing:1.5px;font-weight:700;opacity:.92;'>YCT ACOPIO LECHERO</div>" +
            $"      <div style='margin-top:10px;display:inline-block;background:rgba(255,255,255,.18);padding:5px 12px;border-radius:999px;font-size:11px;font-weight:700;letter-spacing:.8px;'>{badgeText}</div>" +
            $"      <h1 style='margin:14px 0 4px;font-size:22px;font-weight:700;'>{title}</h1>" +
            $"      <div style='font-size:13px;opacity:.9;'>{subtitle}</div>" +
            $"    </div>" +
            $"    <div style='padding:26px 28px;'>" +
            $"      {bodyHtml}" +
            $"    </div>" +
            $"    <div style='padding:14px 28px;background:{bgSoft};font-size:11px;color:#5a6a4e;border-top:1px solid #e5ecdc;'>" +
            $"      Notificación automática · YCT Acopio · {DateTime.Now:dd/MM/yyyy HH:mm}" +
            $"    </div>" +
            $"  </div>" +
            $"</div>";
    }

    private static string StatGrid(params (string label, string value, string color)[] stats)
    {
        var cells = string.Join("", stats.Select(s =>
            $"<td style='padding:14px 10px;text-align:center;background:#f9fbf6;border:1px solid #e5ecdc;border-radius:12px;'>" +
            $"<div style='font-size:10px;text-transform:uppercase;letter-spacing:.6px;color:#6a7a5e;font-weight:700;margin-bottom:6px;'>{s.label}</div>" +
            $"<div style='font-size:20px;font-weight:800;color:{s.color};'>{s.value}</div>" +
            $"</td>"));
        return $"<table cellpadding='0' cellspacing='8' style='width:100%;border-collapse:separate;'><tr>{cells}</tr></table>";
    }
}
