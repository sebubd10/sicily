using BasicCommerce.Application.Features.UserTypes;
using BasicCommerce.Backoffice.Api.Attributes;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.UserTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BasicCommerce.Application.Features.Auth.PermissionCodes;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/user-types")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class UserTypesController : ControllerBase
{
    private readonly IMediator _mediator;
    public UserTypesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [HasPermission(UserTypes.Read)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserTypeResponse>>>> GetAll(
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserTypesQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<UserTypeResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(UserTypes.Read)]
    public async Task<ActionResult<ApiResponse<UserTypeResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserTypeQuery(id), ct);
        return Ok(ApiResponse<UserTypeResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "ChainAdminOnly")]
    [HasPermission(UserTypes.Write)]
    public async Task<ActionResult<ApiResponse<UserTypeResponse>>> Create(
        [FromBody] CreateUserTypeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateUserTypeCommand(
            request.Name, request.Description, request.SortOrder, request.Color), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<UserTypeResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ChainAdminOnly")]
    [HasPermission(UserTypes.Write)]
    public async Task<ActionResult<ApiResponse<UserTypeResponse>>> Update(
        Guid id, [FromBody] UpdateUserTypeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateUserTypeCommand(
            id, request.Name, request.Description, request.SortOrder, request.Color), ct);
        return Ok(ApiResponse<UserTypeResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ChainAdminOnly")]
    [HasPermission(UserTypes.Write)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteUserTypeCommand(id), ct);
        return NoContent();
    }

    /// <summary>Replaces the set of sub-menu items accessible by this user type.</summary>
    [HttpPut("{id:guid}/menus")]
    [Authorize(Policy = "ChainAdminOnly")]
    [HasPermission(UserTypes.Write)]
    public async Task<ActionResult<ApiResponse<UserTypeResponse>>> SetMenus(
        Guid id, [FromBody] SetUserTypeMenusRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SetUserTypeMenusCommand(id, request.SubMenuIds), ct);
        return Ok(ApiResponse<UserTypeResponse>.Ok(result));
    }

    /// <summary>Replaces the set of API permissions granted to this user type.</summary>
    [HttpPut("{id:guid}/permissions")]
    [Authorize(Policy = "ChainAdminOnly")]
    [HasPermission(UserTypes.Write)]
    public async Task<ActionResult<ApiResponse<UserTypeResponse>>> SetPermissions(
        Guid id, [FromBody] SetUserTypePermissionsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SetUserTypePermissionsCommand(id, request.PermissionCodes), ct);
        return Ok(ApiResponse<UserTypeResponse>.Ok(result));
    }
}
