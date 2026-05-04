using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.PurchaseOrders;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.PurchaseOrders.Commands;

public record SubmitPurchaseOrderCommand(Guid PurchaseOrderId) : IRequest<PurchaseOrderResponse>;
public record CancelPurchaseOrderCommand(Guid PurchaseOrderId) : IRequest;

public record ReceivePurchaseOrderCommand(
    Guid PurchaseOrderId,
    IEnumerable<(Guid ProductId, decimal ReceivedQuantity)> Items,
    string? Notes) : IRequest<PurchaseOrderResponse>;

public class ReceivePurchaseOrderCommandValidator : AbstractValidator<ReceivePurchaseOrderCommand>
{
    public ReceivePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item must be received.");
    }
}

public class SubmitPurchaseOrderCommandHandler : IRequestHandler<SubmitPurchaseOrderCommand, PurchaseOrderResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SubmitPurchaseOrderCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PurchaseOrderResponse> Handle(SubmitPurchaseOrderCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var po = await _uow.PurchaseOrders.GetWithItemsAsync(tenantId, request.PurchaseOrderId, ct)
            ?? throw new NotFoundException("PurchaseOrder", request.PurchaseOrderId);
        if (po.TenantId != tenantId) throw new NotFoundException("PurchaseOrder", request.PurchaseOrderId);

        po.Submit();
        await _uow.SaveChangesAsync(ct);
        return PurchaseOrderMapper.ToResponse(po);
    }
}

public class CancelPurchaseOrderCommandHandler : IRequestHandler<CancelPurchaseOrderCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CancelPurchaseOrderCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelPurchaseOrderCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var po = await _uow.PurchaseOrders.GetWithItemsAsync(tenantId, request.PurchaseOrderId, ct)
            ?? throw new NotFoundException("PurchaseOrder", request.PurchaseOrderId);
        if (po.TenantId != tenantId) throw new NotFoundException("PurchaseOrder", request.PurchaseOrderId);

        po.Cancel();
        await _uow.SaveChangesAsync(ct);
    }
}

public class ReceivePurchaseOrderCommandHandler : IRequestHandler<ReceivePurchaseOrderCommand, PurchaseOrderResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ReceivePurchaseOrderCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PurchaseOrderResponse> Handle(ReceivePurchaseOrderCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var po = await _uow.PurchaseOrders.GetWithItemsAsync(tenantId, request.PurchaseOrderId, ct)
            ?? throw new NotFoundException("PurchaseOrder", request.PurchaseOrderId);
        if (po.TenantId != tenantId) throw new NotFoundException("PurchaseOrder", request.PurchaseOrderId);

        if (po.PurchaseOrderStatus is not (PurchaseOrderStatus.Submitted or PurchaseOrderStatus.PartiallyReceived))
            throw new DomainException("Only Submitted or PartiallyReceived orders can be received.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            foreach (var (productId, receivedQty) in request.Items)
            {
                if (receivedQty <= 0) continue;

                var item = po.Items.FirstOrDefault(i => i.ProductId == productId)
                    ?? throw new NotFoundException("PurchaseOrderItem", productId);

                item.Receive(receivedQty);

                var stock = await _uow.WarehouseStockLevels.GetAsync(
                    tenantId, po.WarehouseId, productId, ct);
                if (stock is null)
                {
                    stock = WarehouseStockLevel.Create(tenantId, po.WarehouseId, productId);
                    await _uow.WarehouseStockLevels.AddAsync(stock, ct);
                }

                var before = stock.Quantity;
                stock.Increment(receivedQty);

                var movement = WarehouseMovement.Create(
                    tenantId, po.WarehouseId, productId,
                    WarehouseMovementType.PurchaseOrderReceipt, receivedQty, before,
                    _currentUser.UserId, reference: po.OrderNumber,
                    notes: request.Notes, purchaseOrderId: po.Id);
                await _uow.WarehouseMovements.AddAsync(movement, ct);
            }

            var allReceived = po.Items.All(i => i.IsFullyReceived);
            if (allReceived)
                po.MarkReceived();
            else
                po.MarkPartiallyReceived();

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        return PurchaseOrderMapper.ToResponse(po);
    }
}
