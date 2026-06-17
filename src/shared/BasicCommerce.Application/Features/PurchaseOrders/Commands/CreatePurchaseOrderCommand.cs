using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.PurchaseOrders;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.PurchaseOrders.Commands;

public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    Guid WarehouseId,
    DateTime OrderDate,
    DateTime? ExpectedDate,
    string? Notes,
    string Currency,
    IEnumerable<(Guid ProductId, decimal Quantity, decimal? UnitCost)> Items) : IRequest<PurchaseOrderResponse>;

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
    }
}

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreatePurchaseOrderCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PurchaseOrderResponse> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId, ct)
            ?? throw new NotFoundException("Supplier", request.SupplierId);
        if (supplier.TenantId != tenantId) throw new NotFoundException("Supplier", request.SupplierId);

        var warehouse = await _uow.Warehouses.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);
        if (warehouse.TenantId != tenantId) throw new NotFoundException("Warehouse", request.WarehouseId);

        var orderNumber = await GenerateOrderNumberAsync(tenantId, ct);
        var po = PurchaseOrder.Create(tenantId, orderNumber, request.SupplierId, request.WarehouseId,
            request.OrderDate, request.ExpectedDate, request.Notes, request.Currency);

        foreach (var (productId, quantity, unitCostOverride) in request.Items)
        {
            var product = await _uow.Products.GetByIdAsync(productId, ct)
                ?? throw new NotFoundException("Product", productId);
            if (product.TenantId != tenantId) throw new NotFoundException("Product", productId);

            decimal unitCost;
            if (unitCostOverride.HasValue && unitCostOverride.Value > 0)
            {
                unitCost = unitCostOverride.Value;
            }
            else
            {
                var catalogue = await _uow.SupplierProducts.GetBySupplierAndProductAsync(
                    tenantId, request.SupplierId, productId, ct);
                unitCost = catalogue?.UnitCost
                    ?? throw new DomainException(
                        $"No catalogue price found for product '{product.Name}' " +
                        $"with this supplier. Please provide a unit cost.");
            }

            po.AddItem(productId, quantity, unitCost);
        }

        await _uow.PurchaseOrders.AddAsync(po, ct);
        await _uow.SaveChangesAsync(ct);

        var result = await _uow.PurchaseOrders.GetWithItemsAsync(tenantId, po.Id, ct);
        return PurchaseOrderMapper.ToResponse(result!);
    }

    private async Task<string> GenerateOrderNumberAsync(Guid tenantId, CancellationToken ct)
    {
        var prefix = $"PO-{DateTime.UtcNow:yyyyMM}-";
        var attempt = 0;
        while (true)
        {
            var number = $"{prefix}{(attempt == 0 ? DateTime.UtcNow.Ticks % 100000 : Random.Shared.Next(10000, 99999)):D5}";
            if (!await _uow.PurchaseOrders.OrderNumberExistsAsync(tenantId, number, ct))
                return number;
            attempt++;
        }
    }
}
