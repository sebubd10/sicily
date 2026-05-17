using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Commands;

public record SetLowStockThresholdCommand(Guid StoreId, Guid ProductId, decimal Threshold)
    : IRequest<StockLevelResponse>;

public class SetLowStockThresholdCommandValidator : AbstractValidator<SetLowStockThresholdCommand>
{
    public SetLowStockThresholdCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Threshold).GreaterThanOrEqualTo(0);
    }
}

public class SetLowStockThresholdCommandHandler
    : IRequestHandler<SetLowStockThresholdCommand, StockLevelResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SetLowStockThresholdCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StockLevelResponse> Handle(SetLowStockThresholdCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);
        if (product.TenantId != tenantId) throw new NotFoundException("Product", request.ProductId);

        var stockLevel = await _uow.StockLevels.GetAsync(tenantId, request.StoreId, request.ProductId, ct);
        if (stockLevel is null)
        {
            stockLevel = StockLevel.Create(tenantId, request.StoreId, request.ProductId,
                lowStockThreshold: request.Threshold);
            await _uow.StockLevels.AddAsync(stockLevel, ct);
        }
        else
        {
            stockLevel.SetLowStockThreshold(request.Threshold);
        }

        await _uow.SaveChangesAsync(ct);
        return InventoryMapper.ToResponse(stockLevel, product, string.Empty);
    }
}
