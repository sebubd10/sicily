using BasicCommerce.Application.Features.Auth.Commands;
using BasicCommerce.Contracts.Auth;
using BasicCommerce.Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _config;

    public AuthController(IMediator mediator, IConfiguration config)
    {
        _mediator = mediator;
        _config   = config;
    }

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
        if (!result.Succeeded) return Redirect(BuildErrorRedirect("Google authentication failed."));

        var tenantSlug = result.Properties?.Items.TryGetValue("tenantSlug", out var slug) == true
            ? (string.IsNullOrWhiteSpace(slug) ? null : slug)
            : null;

        var externalId = result.Principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var email      = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)!.Value;
        var firstName  = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty;
        var lastName   = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? string.Empty;

        var auth = await _mediator.Send(
            new ExternalLoginCommand("Google", externalId, email, firstName, lastName, tenantSlug), ct);
        return Redirect(BuildSuccessRedirect(auth));
    }

    // ── Microsoft OAuth ───────────────────────────────────────────────────────

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
        if (!result.Succeeded) return Redirect(BuildErrorRedirect("Microsoft authentication failed."));

        var tenantSlug = result.Properties?.Items.TryGetValue("tenantSlug", out var slug) == true
            ? (string.IsNullOrWhiteSpace(slug) ? null : slug)
            : null;

        var externalId = result.Principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var email      = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)!.Value;
        var firstName  = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty;
        var lastName   = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? string.Empty;

        var auth = await _mediator.Send(
            new ExternalLoginCommand("Microsoft", externalId, email, firstName, lastName, tenantSlug), ct);
        return Redirect(BuildSuccessRedirect(auth));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string BuildSuccessRedirect(AuthResponse auth)
    {
        var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:3000";
        var user = Uri.EscapeDataString(JsonSerializer.Serialize(auth.User));
        return $"{frontendUrl}/auth/callback" +
               $"?accessToken={Uri.EscapeDataString(auth.AccessToken)}" +
               $"&refreshToken={Uri.EscapeDataString(auth.RefreshToken)}" +
               $"&expiresAt={Uri.EscapeDataString(auth.ExpiresAt.ToString("O"))}" +
               $"&user={user}";
    }

    private string BuildErrorRedirect(string message)
    {
        var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:3000";
        return $"{frontendUrl}/login?error={Uri.EscapeDataString(message)}";
    }
}
