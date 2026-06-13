using BasicCommerce.Application.Features.Products;
using BasicCommerce.Application.Features.ProductTags;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Commands;

public record CreateProductCommand(
    string Sku,
    string Barcode,
    string? Plu,
    string Name,
    string NameBn,
    Guid CategoryId,
    decimal Price,
    Guid VatRateId,
    string UnitType,
    bool IsWeightBased,
    bool IsPerishable,
    bool IsAgeRestricted,
    int? AgeRestrictionYears,
    decimal? CostPrice,
    string? Description = null,
    string? UnitLabel = null,
    Guid? ManufacturerId = null,
    IEnumerable<Guid>? TagIds = null,
    bool IsEbtEligible = false,
    bool TrackInventory = true,
    int ReorderLevel = 0) : IRequest<ProductResponse>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Barcode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameBn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.VatRateId).NotEmpty();
        RuleFor(x => x.AgeRestrictionYears)
            .GreaterThanOrEqualTo(0).LessThanOrEqualTo(25)
            .When(x => x.IsAgeRestricted);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateProductCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        if (await _uow.Products.GetBySkuAsync(tenantId, request.Sku, ct) is not null)
            throw new DomainException($"SKU '{request.Sku}' already exists.");

        if (await _uow.Products.GetByBarcodeAsync(tenantId, request.Barcode, ct) is not null)
            throw new DomainException($"Barcode '{request.Barcode}' already exists.");

        if (await _uow.Products.GetByNameAsync(tenantId, request.Name, ct) is not null)
            throw new DomainException($"A product named '{request.Name}' already exists.");

        if (!string.IsNullOrWhiteSpace(request.Plu) &&
            await _uow.Products.GetByPluAsync(tenantId, request.Plu, ct) is not null)
            throw new DomainException($"PLU '{request.Plu}' is already in use.");

        var vatRate = await _uow.VatRates.GetByIdAsync(request.VatRateId, ct)
            ?? throw new NotFoundException("VatRate", request.VatRateId);

        var unitType = Enum.TryParse<UnitType>(request.UnitType, true, out var parsed)
            ? parsed
            : UnitType.Each;

        var price = new Money(request.Price);
        var costPrice = request.CostPrice.HasValue ? new Money(request.CostPrice.Value) : null;
        var product = Product.Create(tenantId, request.Sku, request.Barcode,
            request.Name, request.NameBn, request.CategoryId, price,
            request.VatRateId, unitType, request.IsWeightBased);

        product.UpdateDetails(
            request.Name, request.NameBn, request.Description,
            request.CategoryId, request.VatRateId,
            unitType, request.UnitLabel, request.IsWeightBased,
            request.IsEbtEligible, request.TrackInventory,
            request.ReorderLevel, null, costPrice,
            request.ManufacturerId, request.IsPerishable);

        product.SetPlu(request.Plu);

        if (request.IsAgeRestricted && request.AgeRestrictionYears.HasValue)
            product.SetAgeRestriction(request.AgeRestrictionYears.Value);

        await _uow.Products.AddAsync(product, ct);

        if (request.TagIds is not null)
        {
            var tags = await _uow.ProductTags.GetByIdsAsync(tenantId, request.TagIds, ct);
            ProductTagAssignmentValidator.EnsureNoNewInactiveTags(tags, []);
            product.SetTags(tags);
        }

        await _uow.SaveChangesAsync(ct);

        return ProductMapper.ToResponse(product, vatRate);
    }
}
