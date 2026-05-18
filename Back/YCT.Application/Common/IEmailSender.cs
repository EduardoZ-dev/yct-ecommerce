namespace YCT.Application.Common;

public interface IEmailSender
{
    Task<bool> SendAsync(
        string to,
        string subject,
        string htmlBody,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default);
}

public record EmailAttachment(string FileName, byte[] Content, string ContentType);
