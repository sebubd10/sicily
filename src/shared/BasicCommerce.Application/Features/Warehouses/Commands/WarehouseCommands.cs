using BasicCommerce.Application.Common;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Warehouses;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Warehouses.Commands;

public record CreateWarehouseCommand(
    string Name,
    string Code,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string District,
    string PostalCode,
    string Country,
    string? Phone,
    string? Email,
    bool IsDefault) : IRequest<WarehouseResponse>;

public record UpdateWarehouseCommand(
    Guid WarehouseId,
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string District,
    string PostalCode,
    string Country,
    string? Phone,
    string? Email) : IRequest<WarehouseResponse>;

public record DeactivateWarehouseCommand(Guid WarehouseId) : IRequest;
public record ActivateWarehouseCommand(Guid WarehouseId) : IRequest;
public record DeleteWarehouseCommand(Guid WarehouseId) : IRequest;

public record TransferWarehouseToStoreCommand(
    Guid WarehouseId,
    Guid StoreId,
    Guid ProductId,
    decimal Quantity,
    string? Notes) : IRequest;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District)
            .NotEmpty()
            .Must(BangladeshDistricts.IsValid)
            .WithMessage("District must be a valid Bangladesh district code.");
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
    }
}

public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District)
            .NotEmpty()
            .Must(BangladeshDistricts.IsValid)
            .WithMessage("District must be a valid Bangladesh district code.");
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
    }
}

public class TransferWarehouseToStoreCommandValidator : AbstractValidator<TransferWarehouseToStoreCommand>
{
    public TransferWarehouseToStoreCommandValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateWarehouseCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<WarehouseResponse> Handle(CreateWarehouseCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var address = Address.Create(request.AddressLine1, request.City, request.District,
            request.PostalCode, request.AddressLine2, request.Country);
        var warehouse = Warehouse.Create(tenantId, request.Name, request.Code, address,
            request.Phone, request.Email, request.IsDefault);
        await _uow.Warehouses.AddAsync(warehouse, ct);
        await _uow.SaveChangesAsync(ct);
        return WarehouseMapper.ToResponse(warehouse);
    }
}

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, WarehouseResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateWarehouseCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<WarehouseResponse> Handle(UpdateWarehouseCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var warehouse = await _uow.Warehouses.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);
        if (warehouse.TenantId != tenantId) throw new NotFoundException("Warehouse", request.WarehouseId);

        var address = Address.Create(request.AddressLine1, request.City, request.District,
            request.PostalCode, request.AddressLine2, request.Country);
        warehouse.Update(request.Name, address, request.Phone, request.Email);
        await _uow.SaveChangesAsync(ct);
        return WarehouseMapper.ToResponse(warehouse);
    }
}

public class DeactivateWarehouseCommandHandler : IRequestHandler<DeactivateWarehouseCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateWarehouseCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeactivateWarehouseCommand request, CancellationToken ct)
    {
        var warehouse = await _uow.Warehouses.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);
        if (warehouse.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Warehouse", request.WarehouseId);
        warehouse.Deactivate();
        await _uow.SaveChangesAsync(ct);
    }
}

public class ActivateWarehouseCommandHandler : IRequestHandler<ActivateWarehouseCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ActivateWarehouseCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateWarehouseCommand request, CancellationToken ct)
    {
        var warehouse = await _uow.Warehouses.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);
        if (warehouse.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Warehouse", request.WarehouseId);
        warehouse.Activate();
        await _uow.SaveChangesAsync(ct);
    }
}

public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteWarehouseCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteWarehouseCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var warehouse = await _uow.Warehouses.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);
        if (warehouse.TenantId != tenantId)
            throw new NotFoundException("Warehouse", request.WarehouseId);

        if (warehouse.IsDefault)
            throw new DomainException("Cannot delete the default warehouse. Assign another warehouse as default first.");

        var stockLevels = await _uow.WarehouseStockLevels.GetByWarehouseAsync(tenantId, request.WarehouseId, ct);
        var stockCount = stockLevels.Count(s => s.Quantity > 0);
        if (stockCount > 0)
            throw new DomainException(
                $"Cannot delete: this warehouse has stock for {stockCount} product{(stockCount == 1 ? "" : "s")}. Transfer or write off all stock first.");

        _uow.Warehouses.Remove(warehouse);
        await _uow.SaveChangesAsync(ct);
    }
}

public class TransferWarehouseToStoreCommandHandler : IRequestHandler<TransferWarehouseToStoreCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public TransferWarehouseToStoreCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(TransferWarehouseToStoreCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var warehouse = await _uow.Warehouses.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);
        if (warehouse.TenantId != tenantId) throw new NotFoundException("Warehouse", request.WarehouseId);

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);
        if (product.TenantId != tenantId) throw new NotFoundException("Product", request.ProductId);

        var warehouseStock = await _uow.WarehouseStockLevels.GetAsync(
            tenantId, request.WarehouseId, request.ProductId, ct)
            ?? throw new DomainException("Product not found in warehouse stock.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var whBefore = warehouseStock.Quantity;
            warehouseStock.Decrement(request.Quantity);

            var whMovement = WarehouseMovement.Create(tenantId, request.WarehouseId, request.ProductId,
                WarehouseMovementType.TransferToStore, -request.Quantity, whBefore,
                _currentUser.UserId, notes: request.Notes, relatedStoreId: request.StoreId);
            await _uow.WarehouseMovements.AddAsync(whMovement, ct);

            var storeStock = await _uow.StockLevels.GetAsync(tenantId, request.StoreId, request.ProductId, ct);
            if (storeStock is null)
            {
                storeStock = StockLevel.Create(tenantId, request.StoreId, request.ProductId);
                await _uow.StockLevels.AddAsync(storeStock, ct);
            }
            var storeBefore = storeStock.Quantity;
            storeStock.Increment(request.Quantity);

            var storeMovement = StockMovement.Create(tenantId, request.StoreId, request.ProductId,
                StockMovementType.TransferIn, request.Quantity, storeBefore,
                _currentUser.UserId, notes: request.Notes, relatedStoreId: request.WarehouseId);
            await _uow.StockMovements.AddAsync(storeMovement, ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
