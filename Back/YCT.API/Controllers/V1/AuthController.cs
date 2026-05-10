using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YCT.Application.DTOs;
using YCT.Application.UseCases.Auth.GoogleLogin;
using YCT.Application.UseCases.Auth.Login;
using YCT.Application.UseCases.Auth.Register;

namespace YCT.API.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(new LoginCommand(request.Username, request.Password));
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterCommand(
            request.Username, request.Password, request.FullName, request.Email, request.Phone));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("google")]
    public async Task<IActionResult> Google([FromBody] GoogleLoginRequest request)
    {
        var result = await _mediator.Send(new GoogleLoginCommand(request.IdToken));
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}
