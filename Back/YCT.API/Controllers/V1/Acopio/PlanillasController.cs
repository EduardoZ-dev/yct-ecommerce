using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.Common;
using YCT.Application.UseCases.Acopio.Planillas.Delete;
using YCT.Application.UseCases.Acopio.Planillas.GetAll;
using YCT.Application.UseCases.Acopio.Planillas.GetById;
using YCT.Application.UseCases.Acopio.Planillas.Save;
using YCT.Application.UseCases.Acopio.Planillas.SendEmail;
using YCT.Application.UseCases.Acopio.Planillas.ValidatePlanta;
using YCT.Application.UseCases.Acopio.Planillas.AuthorizeShortage;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1.Acopio;

[ApiController]
[Route("api/acopio/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class PlanillasController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlanillasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllPlanillasQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetPlanillaByIdQuery(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> Create([FromBody] SavePlanillaCommand command)
    {
        var result = await _mediator.Send(command with { Id = null });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> Update(int id, [FromBody] SavePlanillaCommand command)
    {
        var result = await _mediator.Send(command with { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.CanDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeletePlanillaCommand(id));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/email")]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> SendEmail(int id, [FromBody] SendPlanillaEmailCommand command)
    {
        var result = await _mediator.Send(command with { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Operario planta valida total descargado.</summary>
    [HttpPut("{id}/validate-planta")]
    public async Task<IActionResult> ValidatePlanta(int id, [FromBody] ValidatePlantaCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Admin autoriza o rechaza faltante.</summary>
    [HttpPost("{id}/authorize-shortage")]
    [Authorize(Roles = Roles.CanManageUsers)]
    public async Task<IActionResult> AuthorizeShortage(int id, [FromBody] AuthorizeShortageCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>One-click autorización desde email vía token firmado.</summary>
    [HttpGet("quick-authorize")]
    [AllowAnonymous]
    public async Task<IActionResult> QuickAuthorize(
        [FromQuery] string token,
        [FromServices] IEmailActionTokenService tokens)
    {
        if (string.IsNullOrWhiteSpace(token) ||
            !tokens.TryUnprotect(token, out var action, out var rutaId) ||
            action != "authorize-shortage")
        {
            return Content(BuildResultPage(false, "Enlace inválido o expirado",
                "El enlace de autorización es inválido o expiró. Ingresa al panel de YCT Acopio para autorizar manualmente."),
                "text/html");
        }

        var cmd = new AuthorizeShortageCommand { Id = rutaId, Approve = true, Motivo = "Autorizado vía email (acción rápida)" };
        var result = await _mediator.Send(cmd);
        return Content(BuildResultPage(result.Success,
            result.Success ? "Planilla autorizada" : "No se pudo autorizar",
            result.Success
                ? $"La planilla quedó autorizada correctamente. Quedó registrado el motivo: «Autorizado vía email (acción rápida)»."
                : result.Message),
            "text/html");
    }

    private static string BuildResultPage(bool ok, string title, string message)
    {
        var accent = ok ? "#5A9E2F" : "#D32F2F";
        var icon = ok ? "✓" : "⚠";
        return
            "<!DOCTYPE html><html><head><meta charset='utf-8'><title>YCT Acopio</title></head>" +
            "<body style='margin:0;font-family:Segoe UI,Roboto,Arial,sans-serif;background:#f4f6f1;'>" +
            "<div style='max-width:520px;margin:60px auto;background:#fff;border-radius:18px;overflow:hidden;box-shadow:0 6px 24px rgba(30,60,20,.12);border:1px solid #e5ecdc;'>" +
            $"<div style='background:linear-gradient(135deg,{accent},#0F1A0E);padding:34px 28px;color:#fff;text-align:center;'>" +
            $"<div style='font-size:54px;line-height:1;margin-bottom:8px;'>{icon}</div>" +
            $"<h1 style='margin:0;font-size:24px;'>{title}</h1>" +
            "</div>" +
            $"<div style='padding:28px;font-size:15px;color:#1f2a1a;line-height:1.55;'>{message}</div>" +
            "<div style='padding:14px 28px;background:#EDF7E5;font-size:12px;color:#5a6a4e;border-top:1px solid #e5ecdc;text-align:center;'>YCT Acopio Lechero</div>" +
            "</div></body></html>";
    }
}
