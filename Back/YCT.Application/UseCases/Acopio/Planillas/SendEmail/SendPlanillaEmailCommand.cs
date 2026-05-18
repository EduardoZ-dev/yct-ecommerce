using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.Planillas.SendEmail;

public record SendPlanillaEmailCommand(
    int Id,
    string To,
    string? Subject,
    string? Body,
    /// <summary>PDF base64 generado en cliente (sin prefijo data:...).</summary>
    string? PdfBase64,
    string? PdfFileName
) : IRequest<ResponseBase<bool>>;
