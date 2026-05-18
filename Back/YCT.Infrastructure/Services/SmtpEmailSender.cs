using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YCT.Application.Common;

namespace YCT.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendAsync(
        string to,
        string subject,
        string htmlBody,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        var smtp = _config.GetSection("Smtp");
        var host = smtp["Host"];
        var portStr = smtp["Port"];
        var user = smtp["User"];
        var pass = smtp["Password"];
        var from = smtp["From"];
        var fromName = smtp["FromName"] ?? "YCT Acopio";
        var useSsl = bool.TryParse(smtp["UseSsl"], out var ssl) ? ssl : true;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user)
            || string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(from))
        {
            _logger.LogWarning("SMTP no configurado. Falta Host/User/Password/From en appsettings.Smtp");
            return false;
        }

        if (!int.TryParse(portStr, out var port)) port = 587;

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = useSsl,
                Credentials = new NetworkCredential(user, pass),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            using var msg = new MailMessage
            {
                From = new MailAddress(from, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(to);

            if (attachments != null)
            {
                foreach (var att in attachments)
                {
                    var stream = new MemoryStream(att.Content);
                    msg.Attachments.Add(new Attachment(stream, att.FileName, att.ContentType));
                }
            }

            await client.SendMailAsync(msg, cancellationToken);
            _logger.LogInformation("Email enviado a {To} · {Subject}", to, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email a {To}", to);
            return false;
        }
    }
}
