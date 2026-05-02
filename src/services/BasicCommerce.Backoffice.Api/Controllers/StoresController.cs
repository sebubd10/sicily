using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Stores;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/stores")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class StoresController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public StoresController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public IActionResult GetAll() =>
        StatusCode(501, ApiResponse<object>.Fail("GetStoresQuery not yet implemented."));

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) =>
        StatusCode(501, ApiResponse<object>.Fail("GetStoreQuery not yet implemented."));

    [HttpPost]
    [Authorize(Policy = "ChainAdminOnly")]
    public IActionResult Create([FromBody] CreateStoreRequest request) =>
        StatusCode(501, ApiResponse<object>.Fail("CreateStoreCommand not yet implemented."));

    [HttpGet("{storeId:guid}/terminals")]
    public IActionResult GetTerminals(Guid storeId) =>
        StatusCode(501, ApiResponse<object>.Fail("GetTerminalsQuery not yet implemented."));
}
