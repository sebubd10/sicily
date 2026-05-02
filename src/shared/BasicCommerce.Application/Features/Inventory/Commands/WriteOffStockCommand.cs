using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Commands;

public record WriteOffStockCommand(
    Guid StoreId,
    Guid ProductId,
    decimal Quantity,
    string Reason) : IRequest<StockLevelResponse>;

public class WriteOffStockCommandValidator : AbstractValidator<WriteOffStockCommand>
{
    public WriteOffStockCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class WriteOffStockCommandHandler : IRequestHandler<WriteOffStockCommand, StockLevelResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public WriteOffStockCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StockLevelResponse> Handle(WriteOffStockCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var stockLevel = await _uow.StockLevels.GetAsync(tenantId, request.StoreId, request.ProductId, ct)
            ?? throw new DomainException("No stock record found for this product at the specified store.");

        if (stockLevel.TenantId != tenantId)
            throw new DomainException("Stock level not found.");

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        var before = stockLevel.Quantity;
        stockLevel.Decrement(request.Quantity);

        var movement = StockMovement.Create(tenantId, request.StoreId, request.ProductId,
            StockMovementType.WriteOff, -request.Quantity, before,
            _currentUser.UserId, notes: request.Reason);
        await _uow.StockMovements.AddAsync(movement, ct);

        await _uow.SaveChangesAsync(ct);
        return InventoryMapper.ToResponse(stockLevel, product, string.Empty);
    }
}
