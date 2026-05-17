using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Commands;

public record AdjustStockCommand(
    Guid StoreId,
    Guid ProductId,
    decimal NewQuantity,
    string Notes) : IRequest<StockLevelResponse>;

public class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(500);
    }
}

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, StockLevelResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AdjustStockCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StockLevelResponse> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);
        if (product.TenantId != tenantId) throw new NotFoundException("Product", request.ProductId);

        var stockLevel = await _uow.StockLevels.GetAsync(tenantId, request.StoreId, request.ProductId, ct);
        if (stockLevel is null)
        {
            stockLevel = StockLevel.Create(tenantId, request.StoreId, request.ProductId);
            await _uow.StockLevels.AddAsync(stockLevel, ct);
        }

        var before = stockLevel.Quantity;
        var delta = request.NewQuantity - before;

        stockLevel.AdjustCount(request.NewQuantity, _currentUser.UserId);

        var movement = StockMovement.Create(tenantId, request.StoreId, request.ProductId,
            StockMovementType.Adjustment, delta, before,
            _currentUser.UserId, notes: request.Notes);
        await _uow.StockMovements.AddAsync(movement, ct);

        await _uow.SaveChangesAsync(ct);
        return InventoryMapper.ToResponse(stockLevel, product, string.Empty);
    }
}
