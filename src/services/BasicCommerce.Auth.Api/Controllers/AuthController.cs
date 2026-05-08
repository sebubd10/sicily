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
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _mediator.Send(
            new RefreshTokenCommand(request.RefreshToken, ip), ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    // ── Google OAuth ──────────────────────────────────────────────────────────

    /// <summary>Initiates Google OAuth. Pass tenantSlug for admin users; omit for customers.</summary>
    [HttpGet("google")]
    public IActionResult GoogleLogin([FromQuery] string? tenantSlug = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback));
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl,
            Items = { ["tenantSlug"] = tenantSlug ?? string.Empty }
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback(CancellationToken ct)
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded) return BadRequest("Google authentication failed.");

        var tenantSlug = result.Properties?.Items.TryGetValue("tenantSlug", out var slug) == true
            ? (string.IsNullOrWhiteSpace(slug) ? null : slug)
            : null;

        var externalId = result.Principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var email      = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)!.Value;
        var firstName  = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty;
        var lastName   = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? string.Empty;

        var authResult = await _mediator.Send(
            new ExternalLoginCommand("Google", externalId, email, firstName, lastName, tenantSlug), ct);
        return Ok(ApiResponse<AuthResponse>.Ok(authResult));
    }

    // ── Microsoft OAuth ───────────────────────────────────────────────────────

    /// <summary>Initiates Microsoft OAuth. Pass tenantSlug for admin users; omit for customers.</summary>
    [HttpGet("microsoft")]
    public IActionResult MicrosoftLogin([FromQuery] string? tenantSlug = null)
    {
        var redirectUrl = Url.Action(nameof(MicrosoftCallback));
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl,
            Items = { ["tenantSlug"] = tenantSlug ?? string.Empty }
        };
        return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
    }

    [HttpGet("microsoft/callback")]
    public async Task<IActionResult> MicrosoftCallback(CancellationToken ct)
    {
        var result = await HttpContext.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);
        if (!result.Succeeded) return BadRequest("Microsoft authentication failed.");

        var tenantSlug = result.Properties?.Items.TryGetValue("tenantSlug", out var slug) == true
            ? (string.IsNullOrWhiteSpace(slug) ? null : slug)
            : null;

        var externalId = result.Principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var email      = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)!.Value;
        var firstName  = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty;
        var lastName   = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? string.Empty;

        var authResult = await _mediator.Send(
            new ExternalLoginCommand("Microsoft", externalId, email, firstName, lastName, tenantSlug), ct);
        return Ok(ApiResponse<AuthResponse>.Ok(authResult));
    }
}
