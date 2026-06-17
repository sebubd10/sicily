using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.PurchaseOrders;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.PurchaseOrders.Commands;

public record UpdatePurchaseOrderCommand(
    Guid PurchaseOrderId,
    Guid SupplierId,
    Guid WarehouseId,
    DateTime OrderDate,
    DateTime? ExpectedDate,
    string? Notes,
    string Currency,
    IEnumerable<(Guid ProductId, decimal Quantity, decimal? UnitCost)> Items) : IRequest<PurchaseOrderResponse>;

public class UpdatePurchaseOrderCommandValidator : AbstractValidator<UpdatePurchaseOrderCommand>
{
    public UpdatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one line item is required.");
    }
}

public class UpdatePurchaseOrderCommandHandler : IRequestHandler<UpdatePurchaseOrderCommand, PurchaseOrderResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdatePurchaseOrderCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PurchaseOrderResponse> Handle(UpdatePurchaseOrderCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var po = await _uow.PurchaseOrders.GetWithItemsAsync(tenantId, request.PurchaseOrderId, ct)
            ?? throw new NotFoundException("PurchaseOrder", request.PurchaseOrderId);
        if (po.TenantId != tenantId) throw new NotFoundException("PurchaseOrder", request.PurchaseOrderId);

        var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId, ct)
            ?? throw new NotFoundException("Supplier", request.SupplierId);
        if (supplier.TenantId != tenantId) throw new NotFoundException("Supplier", request.SupplierId);

        var warehouse = await _uow.Warehouses.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);
        if (warehouse.TenantId != tenantId) throw new NotFoundException("Warehouse", request.WarehouseId);

        po.UpdateDetails(request.SupplierId, request.WarehouseId,
            request.OrderDate, request.ExpectedDate, request.Notes, request.Currency);

        var oldItems = po.Items.ToList();
        po.ClearItems();
        _uow.PurchaseOrders.RemoveItems(oldItems);

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

        _uow.PurchaseOrders.AddItems(po.Items.ToList());

        await _uow.SaveChangesAsync(ct);

        var result = await _uow.PurchaseOrders.GetWithItemsAsync(tenantId, po.Id, ct);
        return PurchaseOrderMapper.ToResponse(result!);
    }
}
