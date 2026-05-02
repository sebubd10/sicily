using BasicCommerce.Application.Features.Auth.Commands;
using BasicCommerce.Contracts.Auth;
using BasicCommerce.Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Auth.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new LoginCommand(request.Email, request.Password, request.TenantSlug), ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(
        [FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        // TODO: implement refresh token handler
        return StatusCode(501, ApiResponse<AuthResponse>.Fail("Not implemented yet."));
    }

    [HttpGet("google")]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl, CancellationToken ct)
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded) return BadRequest("Google authentication failed.");

        var externalId = result.Principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var email = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)!.Value;
        var firstName = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty;
        var lastName = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? string.Empty;

        var authResult = await _mediator.Send(
            new ExternalLoginCommand("Google", externalId, email, firstName, lastName, null), ct);

        return Ok(ApiResponse<AuthResponse>.Ok(authResult));
    }

    [HttpGet("microsoft")]
    public IActionResult MicrosoftLogin([FromQuery] string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(MicrosoftCallback), new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
    }

    [HttpGet("microsoft/callback")]
    public async Task<IActionResult> MicrosoftCallback([FromQuery] string? returnUrl, CancellationToken ct)
    {
        var result = await HttpContext.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);
        if (!result.Succeeded) return BadRequest("Microsoft authentication failed.");

        var externalId = result.Principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var email = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)!.Value;
        var firstName = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty;
        var lastName = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? string.Empty;

        var authResult = await _mediator.Send(
            new ExternalLoginCommand("Microsoft", externalId, email, firstName, lastName, null), ct);

        return Ok(ApiResponse<AuthResponse>.Ok(authResult));
    }
}
