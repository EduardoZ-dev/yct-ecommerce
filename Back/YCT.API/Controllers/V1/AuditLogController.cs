using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.AuditLogs.GetAuditLogs;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.CanManageUsers)]
public class AuditLogController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] bool? success = null)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(
            page, pageSize, action, entityType, userId, from, to, success));
        return Ok(result);
    }
}
