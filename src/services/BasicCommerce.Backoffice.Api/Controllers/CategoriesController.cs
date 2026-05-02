using BasicCommerce.Application.Features.Products.Commands;
using BasicCommerce.Application.Features.Products.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponse>>>> GetAll(
        [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(includeInactive), ct);
        return Ok(ApiResponse<IEnumerable<CategoryResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCategoryQuery(id), ct);
        return Ok(ApiResponse<CategoryResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> Create(
        [FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCategoryCommand(
            request.Name,
            request.NameBn,
            request.ParentCategoryId,
            request.Description), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<CategoryResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> Update(
        Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateCategoryCommand(
            id,
            request.Name,
            request.NameBn,
            request.Description,
            request.SortOrder), ct);
        return Ok(ApiResponse<CategoryResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateCategoryCommand(id), ct);
        return NoContent();
    }
}

public record CreateCategoryRequest(
    string Name,
    string NameBn,
    Guid? ParentCategoryId = null,
    string? Description = null);
