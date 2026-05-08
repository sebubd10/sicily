using BasicCommerce.Application.Features.UserTypes;
using BasicCommerce.Application.Features.Users.Commands;
using BasicCommerce.Application.Features.Users.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.UserTypes;
using BasicCommerce.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserListResponse>>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUsersQuery(page, pageSize), ct);
        return Ok(ApiResponse<IEnumerable<UserListResponse>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<UserListResponse>>> Create(
        [FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.Role,
            request.StoreId,
            request.PhoneNumber), ct);
        return CreatedAtAction(null, ApiResponse<UserListResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateUserCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpPut("{id:guid}/unlock")]
    public async Task<ActionResult<ApiResponse<object>>> Unlock(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new UnlockUserCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    /// <summary>Assigns or removes a user type from a user.</summary>
    [HttpPut("{id:guid}/user-type")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<IActionResult> AssignUserType(
        Guid id, [FromBody] AssignUserTypeRequest request, CancellationToken ct)
    {
        await _mediator.Send(new AssignUserTypeCommand(id, request.UserTypeId), ct);
        return Ok(ApiResponse<object>.Ok(null!));
    }
}
