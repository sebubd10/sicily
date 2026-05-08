using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Commands;

public record ReceiveStockBatchCommand(
    Guid StoreId,
    Guid ProductId,
    decimal Quantity,
    DateTime? ExpiryDate,
    string? LotNumber,
    decimal? UnitCost,
    Guid? PurchaseOrderId,
    string? Reference,
    string? Notes) : IRequest<StockBatchResponse>;

public class ReceiveStockBatchCommandValidator : AbstractValidator<ReceiveStockBatchCommand>
{
    public ReceiveStockBatchCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThan(0).When(x => x.UnitCost.HasValue);
    }
}

public class ReceiveStockBatchCommandHandler : IRequestHandler<ReceiveStockBatchCommand, StockBatchResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ReceiveStockBatchCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StockBatchResponse> Handle(ReceiveStockBatchCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);
        if (product.TenantId != tenantId) throw new NotFoundException("Product", request.ProductId);

        if (!product.IsPerishable)
            throw new DomainException("Batch tracking is only available for perishable products.");

        var batch = StockBatch.Create(tenantId, request.StoreId, request.ProductId,
            request.Quantity, request.ExpiryDate, request.LotNumber,
            request.UnitCost, request.PurchaseOrderId);
        await _uow.StockBatches.AddAsync(batch, ct);

        var stockLevel = await _uow.StockLevels.GetAsync(tenantId, request.StoreId, request.ProductId, ct);
        if (stockLevel is null)
        {
            stockLevel = StockLevel.Create(tenantId, request.StoreId, request.ProductId);
            await _uow.StockLevels.AddAsync(stockLevel, ct);
        }

        var before = stockLevel.Quantity;
        stockLevel.Increment(request.Quantity);
        _uow.StockLevels.Update(stockLevel);

        var movement = StockMovement.Create(tenantId, request.StoreId, request.ProductId,
            StockMovementType.Receive, request.Quantity, before,
            _currentUser.UserId, request.Reference, request.Notes);
        await _uow.StockMovements.AddAsync(movement, ct);

        await _uow.SaveChangesAsync(ct);

        return BatchMapper.ToResponse(batch, product);
    }
}
