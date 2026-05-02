using BasicCommerce.Application.Features.Products.Commands;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
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
        return CreatedAtAction(null, ApiResponse<CategoryResponse>.Ok(result));
    }
}

public record CreateCategoryRequest(
    string Name,
    string NameBn,
    Guid? ParentCategoryId = null,
    string? Description = null);
