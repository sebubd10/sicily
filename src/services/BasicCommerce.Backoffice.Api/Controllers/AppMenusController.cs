using BasicCommerce.Application.Features.UserTypes;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.UserTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/menus")]
[Authorize]
public class AppMenusController : ControllerBase
{
    private readonly IMediator _mediator;
    public AppMenusController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns menus and sub-menus accessible to the current user.
    /// Used by the dashboard to build navigation.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MenuResponse>>>> GetMenus(
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMenusQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<MenuResponse>>.Ok(result));
    }

    /// <summary>Returns all menus and sub-menus (admin use — for configuring user types).</summary>
    [HttpGet("all")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MenuResponse>>>> GetAllMenus(
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMenusQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<MenuResponse>>.Ok(result));
    }
}

[ApiController]
[Route("api/permissions")]
[Authorize(Policy = "ChainAdminOnly")]
public class ApiPermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ApiPermissionsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns all defined API permissions, optionally filtered by group.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ApiPermissionResponse>>>> GetAll(
        [FromQuery] string? group, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetApiPermissionsQuery(group), ct);
        return Ok(ApiResponse<IReadOnlyList<ApiPermissionResponse>>.Ok(result));
    }
}
