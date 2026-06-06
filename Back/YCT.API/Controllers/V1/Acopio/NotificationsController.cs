using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.Common;
using YCT.Application.UseCases.Acopio.Notifications.GetAll;
using YCT.Application.UseCases.Acopio.Notifications.MarkRead;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1.Acopio;

[ApiController]
[Route("api/acopio/[controller]")]
[Authorize(Roles = Roles.CanManageUsers)]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyUnread = false, [FromQuery] int take = 50)
    {
        var result = await _mediator.Send(new GetAllNotificationsQuery { OnlyUnread = onlyUnread, Take = take });
        return Ok(result);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var result = await _mediator.Send(new MarkReadCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var result = await _mediator.Send(new MarkReadCommand { All = true });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("test-email")]
    [AllowAnonymous]
    public async Task<IActionResult> TestEmail(
        [FromServices] IEmailSender emailSender,
        [FromQuery] string to = "yairevarduardozeq@gmail.com")
    {
        var ok = await emailSender.SendAsync(
            to,
            "Test SMTP · YCT Acopio",
            "<h2>Prueba SMTP</h2><p>Si lees esto, el SMTP de YCT funciona correctamente.</p><p><b>Hora:</b> " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "</p>",
            null);
        return ok
            ? Ok(new { success = true, message = $"Email enviado a {to}" })
            : BadRequest(new { success = false, message = "Falla SMTP. Revisa logs del backend." });
    }
}
