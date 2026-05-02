using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Commands;

public record CreateCategoryCommand(
    string Name,
    string NameBn,
    Guid? ParentCategoryId = null,
    string? Description = null) : IRequest<CategoryResponse>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameBn).NotEmpty().MaximumLength(100);
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCategoryCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CategoryResponse> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        if (await _uow.Categories.NameExistsAsync(tenantId, request.Name, ct: ct))
            throw new DomainException($"Category '{request.Name}' already exists.");

        if (request.ParentCategoryId.HasValue)
        {
            var parent = await _uow.Categories.GetByIdAsync(request.ParentCategoryId.Value, ct)
                ?? throw new NotFoundException("Category", request.ParentCategoryId.Value);

            if (parent.TenantId != tenantId)
                throw new NotFoundException("Category", request.ParentCategoryId.Value);
        }

        var category = Category.Create(tenantId, request.Name, request.NameBn,
            request.ParentCategoryId, request.Description);

        await _uow.Categories.AddAsync(category, ct);
        await _uow.SaveChangesAsync(ct);

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.NameBn,
            category.Description,
            category.ParentCategoryId,
            null,
            category.SortOrder,
            category.IsActive,
            0);
    }
}
