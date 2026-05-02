using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YCT.Application.UseCases.Dashboard.GetMetrics;
using YCT.Application.UseCases.Dashboard.GetRevenueProjection;
using YCT.Domain.Common;

namespace YCT.API.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.AdminPanel)]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var result = await _mediator.Send(new GetMetricsQuery());
        return Ok(result);
    }

    [HttpGet("revenue-projection")]
    public async Task<IActionResult> GetRevenueProjection()
    {
        var result = await _mediator.Send(new GetRevenueProjectionQuery());
        return Ok(result);
    }
}
