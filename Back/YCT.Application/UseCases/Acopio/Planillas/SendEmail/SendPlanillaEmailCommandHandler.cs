using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Planillas.SendEmail;

public class SendPlanillaEmailCommandHandler : IRequestHandler<SendPlanillaEmailCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<Ruta> _rutaRepository;
    private readonly IEmailSender _emailSender;
    private readonly IAuditLogger _audit;

    public SendPlanillaEmailCommandHandler(
        IGenericRepository<Ruta> rutaRepository,
        IEmailSender emailSender,
        IAuditLogger audit)
    {
        _rutaRepository = rutaRepository;
        _emailSender = emailSender;
        _audit = audit;
    }

    public async Task<ResponseBase<bool>> Handle(SendPlanillaEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.To) || !request.To.Contains('@'))
            return ResponseBase<bool>.Fail("Correo destinatario inválido");

        var ruta = await _rutaRepository.GetByIdAsync(request.Id);
        if (ruta == null) return ResponseBase<bool>.Fail("Planilla no encontrada");

        var subject = string.IsNullOrWhiteSpace(request.Subject)
            ? $"Planilla acopio · {ruta.Codigo} · {ruta.Fecha:dd/MM/yyyy}"
            : request.Subject;

        var body = string.IsNullOrWhiteSpace(request.Body)
            ? $@"<p>Hola,</p>
<p>Adjunto la planilla de acopio de leche.</p>
<ul>
  <li><strong>Ruta:</strong> {ruta.Codigo}</li>
  <li><strong>Fecha:</strong> {ruta.Fecha:dd/MM/yyyy}</li>
  <li><strong>Total litros:</strong> {ruta.TotalLitrosChofer} L</li>
</ul>
<p>Saludos,<br/>YCT Distribuciones</p>"
            : request.Body;

        var attachments = new List<EmailAttachment>();
        if (!string.IsNullOrWhiteSpace(request.PdfBase64))
        {
            try
            {
                var bytes = Convert.FromBase64String(request.PdfBase64);
                var name = string.IsNullOrWhiteSpace(request.PdfFileName)
                    ? $"planilla-{ruta.Codigo}-{ruta.Fecha:yyyyMMdd}.pdf"
                    : request.PdfFileName;
                attachments.Add(new EmailAttachment(name, bytes, "application/pdf"));
            }
            catch (FormatException)
            {
                return ResponseBase<bool>.Fail("PDF adjunto inválido (base64 corrupto)");
            }
        }

        var ok = await _emailSender.SendAsync(request.To, subject, body, attachments, cancellationToken);
        if (!ok)
            return ResponseBase<bool>.Fail("No se pudo enviar el correo. Verifica configuración SMTP en el servidor.");

        await _audit.LogAsync("SendEmail", "Planilla", ruta.Id,
            $"Planilla {ruta.Codigo} enviada por email a {request.To}",
            new { request.To, request.Subject, attached = attachments.Count > 0 },
            ct: cancellationToken);

        return ResponseBase<bool>.Ok(true, "Correo enviado");
    }
}
