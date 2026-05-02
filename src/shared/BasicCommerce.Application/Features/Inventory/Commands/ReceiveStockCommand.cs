using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Commands;

public record ReceiveStockCommand(
    Guid StoreId,
    Guid ProductId,
    decimal Quantity,
    string? Reference,
    string? Notes) : IRequest<StockLevelResponse>;

public class ReceiveStockCommandValidator : AbstractValidator<ReceiveStockCommand>
{
    public ReceiveStockCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class ReceiveStockCommandHandler : IRequestHandler<ReceiveStockCommand, StockLevelResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ReceiveStockCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StockLevelResponse> Handle(ReceiveStockCommand request, CancellationToken ct)
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
        stockLevel.Increment(request.Quantity);

        var movement = StockMovement.Create(tenantId, request.StoreId, request.ProductId,
            StockMovementType.Receive, request.Quantity, before,
            _currentUser.UserId, request.Reference, request.Notes);
        await _uow.StockMovements.AddAsync(movement, ct);

        await _uow.SaveChangesAsync(ct);
        return InventoryMapper.ToResponse(stockLevel, product, string.Empty);
    }
}
