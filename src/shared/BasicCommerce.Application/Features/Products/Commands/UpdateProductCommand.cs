using BasicCommerce.Application.Features.Products;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Commands;

public record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string NameBn,
    string? Description,
    Guid CategoryId,
    decimal Price,
    Guid VatRateId,
    string UnitType,
    string? UnitLabel,
    bool IsWeightBased,
    bool IsPerishable,
    bool IsAgeRestricted,
    int? AgeRestrictionYears,
    bool IsEbtEligible,
    bool TrackInventory,
    int ReorderLevel,
    string? ImageUrl,
    decimal? CostPrice,
    Guid? ManufacturerId = null,
    IEnumerable<Guid>? TagIds = null,
    string? Plu = null) : IRequest<ProductResponse>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameBn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.VatRateId).NotEmpty();
        RuleFor(x => x.AgeRestrictionYears)
            .GreaterThanOrEqualTo(0).LessThanOrEqualTo(25)
            .When(x => x.IsAgeRestricted);
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _uow.Products.GetWithTagsAsync(_currentUser.TenantId, request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (product.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Product", request.ProductId);

        var tenantId = _currentUser.TenantId;

        var skuConflict = await _uow.Products.GetBySkuAsync(tenantId, product.Sku, ct);
        if (skuConflict is not null && skuConflict.Id != request.ProductId)
            throw new DomainException($"SKU '{product.Sku}' is already in use by another product.");

        var barcodeConflict = await _uow.Products.GetByBarcodeAsync(tenantId, product.Barcode, ct);
        if (barcodeConflict is not null && barcodeConflict.Id != request.ProductId)
            throw new DomainException($"Barcode '{product.Barcode}' is already in use by another product.");

        if (!string.IsNullOrWhiteSpace(request.Plu))
        {
            var pluConflict = await _uow.Products.GetByPluAsync(tenantId, request.Plu, ct);
            if (pluConflict is not null && pluConflict.Id != request.ProductId)
                throw new DomainException($"PLU '{request.Plu}' is already in use by another product.");
        }

        var vatRate = await _uow.VatRates.GetByIdAsync(request.VatRateId, ct)
            ?? throw new NotFoundException("VatRate", request.VatRateId);

        var unitType = Enum.TryParse<UnitType>(request.UnitType, true, out var parsed)
            ? parsed : UnitType.Each;

        var costPrice = request.CostPrice.HasValue ? new Money(request.CostPrice.Value) : null;

        product.UpdateDetails(
            request.Name, request.NameBn, request.Description,
            request.CategoryId, request.VatRateId,
            unitType, request.UnitLabel, request.IsWeightBased,
            request.IsEbtEligible, request.TrackInventory,
            request.ReorderLevel, request.ImageUrl, costPrice,
            request.ManufacturerId, request.IsPerishable);

        product.UpdatePrice(new Money(request.Price));
        product.SetPlu(request.Plu);

        if (request.IsAgeRestricted && request.AgeRestrictionYears.HasValue)
            product.SetAgeRestriction(request.AgeRestrictionYears.Value);
        else if (!request.IsAgeRestricted)
            product.RemoveAgeRestriction();

        if (request.TagIds is not null)
        {
            var tags = await _uow.ProductTags.GetByIdsAsync(
                _currentUser.TenantId, request.TagIds, ct);
            product.SetTags(tags);
        }

        await _uow.SaveChangesAsync(ct);

        var category = await _uow.Categories.GetByIdAsync(product.CategoryId, ct);
        return ProductMapper.ToResponse(product, vatRate, category?.Name);
    }
}
