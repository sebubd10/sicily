using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Commands;

public record TransferStockCommand(
    Guid SourceStoreId,
    Guid DestinationStoreId,
    Guid ProductId,
    decimal Quantity,
    string? Notes) : IRequest<StockLevelResponse>;

public class TransferStockCommandValidator : AbstractValidator<TransferStockCommand>
{
    public TransferStockCommandValidator()
    {
        RuleFor(x => x.SourceStoreId).NotEmpty();
        RuleFor(x => x.DestinationStoreId).NotEmpty()
            .NotEqual(x => x.SourceStoreId).WithMessage("Source and destination stores must differ.");
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class TransferStockCommandHandler : IRequestHandler<TransferStockCommand, StockLevelResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public TransferStockCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StockLevelResponse> Handle(TransferStockCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var sourceStore = await _uow.Stores.GetByIdAsync(request.SourceStoreId, ct)
            ?? throw new NotFoundException("Store", request.SourceStoreId);
        if (sourceStore.TenantId != tenantId) throw new NotFoundException("Store", request.SourceStoreId);

        var destStore = await _uow.Stores.GetByIdAsync(request.DestinationStoreId, ct)
            ?? throw new NotFoundException("Store", request.DestinationStoreId);
        if (destStore.TenantId != tenantId) throw new NotFoundException("Store", request.DestinationStoreId);

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        var sourceLevel = await _uow.StockLevels.GetAsync(tenantId, request.SourceStoreId, request.ProductId, ct)
            ?? throw new DomainException("No stock at source store for this product.");

        var sourceBefore = sourceLevel.Quantity;
        sourceLevel.Decrement(request.Quantity);

        var destLevel = await _uow.StockLevels.GetAsync(tenantId, request.DestinationStoreId, request.ProductId, ct);
        if (destLevel is null)
        {
            destLevel = StockLevel.Create(tenantId, request.DestinationStoreId, request.ProductId);
            await _uow.StockLevels.AddAsync(destLevel, ct);
        }
        var destBefore = destLevel.Quantity;
        destLevel.Increment(request.Quantity);

        await _uow.StockMovements.AddAsync(StockMovement.Create(tenantId, request.SourceStoreId,
            request.ProductId, StockMovementType.TransferOut, -request.Quantity, sourceBefore,
            _currentUser.UserId, notes: request.Notes, relatedStoreId: request.DestinationStoreId), ct);

        await _uow.StockMovements.AddAsync(StockMovement.Create(tenantId, request.DestinationStoreId,
            request.ProductId, StockMovementType.TransferIn, request.Quantity, destBefore,
            _currentUser.UserId, notes: request.Notes, relatedStoreId: request.SourceStoreId), ct);

        await _uow.SaveChangesAsync(ct);
        return InventoryMapper.ToResponse(sourceLevel, product, sourceStore.Name);
    }
}
