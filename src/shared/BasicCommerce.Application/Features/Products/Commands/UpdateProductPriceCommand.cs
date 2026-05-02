using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Commands;

public record UpdateProductPriceCommand(Guid ProductId, decimal NewPrice) : IRequest<ProductResponse>;

public class UpdateProductPriceCommandValidator : AbstractValidator<UpdateProductPriceCommand>
{
    public UpdateProductPriceCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.NewPrice).GreaterThan(0);
    }
}

public class UpdateProductPriceCommandHandler : IRequestHandler<UpdateProductPriceCommand, ProductResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductPriceCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductResponse> Handle(UpdateProductPriceCommand request, CancellationToken ct)
    {
        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (product.TenantId != _currentUser.TenantId)
            throw new UnauthorizedException("Product not found.");

        product.UpdatePrice(new Money(request.NewPrice));
        await _uow.SaveChangesAsync(ct);

        var vatRate = await _uow.VatRates.GetByIdAsync(product.VatRateId, ct);

        return new ProductResponse(
            product.Id,
            product.Sku,
            product.Barcode,
            product.Plu,
            product.Name,
            product.NameBn,
            product.CategoryId.ToString(),
            string.Empty,
            product.Price.Amount,
            product.Price.Currency,
            vatRate?.Rate ?? 0,
            product.UnitType.ToString(),
            product.IsWeightBased,
            product.IsAgeRestricted,
            product.AgeRestrictionYears,
            product.IsActive,
            product.ImageUrl);
    }
}
