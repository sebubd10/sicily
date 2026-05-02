using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
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
    public IActionResult GetAll([FromQuery] PaginatedRequest request) =>
        StatusCode(501, ApiResponse<object>.Fail("GetUsersQuery not yet implemented."));

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) =>
        StatusCode(501, ApiResponse<object>.Fail("GetUserQuery not yet implemented."));

    [HttpPost]
    [Authorize(Policy = "ChainAdminOnly")]
    public IActionResult Create([FromBody] CreateUserRequest request) =>
        StatusCode(501, ApiResponse<object>.Fail("CreateUserCommand not yet implemented."));

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "ChainAdminOnly")]
    public IActionResult Deactivate(Guid id) =>
        StatusCode(501, ApiResponse<object>.Fail("DeactivateUserCommand not yet implemented."));

    [HttpPut("{id:guid}/unlock")]
    public IActionResult Unlock(Guid id) =>
        StatusCode(501, ApiResponse<object>.Fail("UnlockUserCommand not yet implemented."));
}
