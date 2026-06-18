using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Commands;

public record DeactivateProductCommand(Guid ProductId) : IRequest;
public record ActivateProductCommand(Guid ProductId) : IRequest;
public record DeleteProductCommand(Guid ProductId) : IRequest;

public class DeactivateProductCommandHandler : IRequestHandler<DeactivateProductCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateProductCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeactivateProductCommand request, CancellationToken ct)
    {
        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (product.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Product", request.ProductId);

        product.Deactivate();
        await _uow.SaveChangesAsync(ct);
    }
}

public class ActivateProductCommandHandler : IRequestHandler<ActivateProductCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ActivateProductCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateProductCommand request, CancellationToken ct)
    {
        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (product.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Product", request.ProductId);

        product.Activate();
        await _uow.SaveChangesAsync(ct);
    }
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteProductCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (product.TenantId != tenantId)
            throw new NotFoundException("Product", request.ProductId);

        var reasons = new List<string>();

        if (await _uow.StockLevels.HasStockForProductAsync(tenantId, request.ProductId, ct))
            reasons.Add("the product has stock remaining in one or more stores");

        if (await _uow.WarehouseStockLevels.HasStockForProductAsync(tenantId, request.ProductId, ct))
            reasons.Add("the product has stock remaining in one or more warehouses");

        if (await _uow.PurchaseOrders.HasOpenOrdersForProductAsync(tenantId, request.ProductId, ct))
            reasons.Add("the product is referenced by open purchase orders (Draft, Submitted, or Partially Received)");

        if (await _uow.StockBatches.HasActiveBatchesForProductAsync(tenantId, request.ProductId, ct))
            reasons.Add("the product has active stock batches with remaining quantity");

        if (reasons.Count > 0)
            throw new DomainException(
                $"Cannot delete product '{product.Name}': {string.Join("; ", reasons)}.");

        product.SoftDelete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
