using BasicCommerce.Application.Features.SupplierReturns;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.SupplierReturns;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.SupplierReturns.Commands;

public record CreateSupplierReturnCommand(
    Guid SupplierId,
    Guid StoreId,
    Guid? PurchaseOrderId,
    string? Notes) : IRequest<SupplierReturnResponse>;

public class CreateSupplierReturnCommandValidator : AbstractValidator<CreateSupplierReturnCommand>
{
    public CreateSupplierReturnCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.StoreId).NotEmpty();
    }
}

public class CreateSupplierReturnCommandHandler
    : IRequestHandler<CreateSupplierReturnCommand, SupplierReturnResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateSupplierReturnCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierReturnResponse> Handle(
        CreateSupplierReturnCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId, ct)
            ?? throw new NotFoundException("Supplier", request.SupplierId);
        if (supplier.TenantId != tenantId) throw new NotFoundException("Supplier", request.SupplierId);

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        var supplierReturn = SupplierReturn.Create(
            tenantId, request.SupplierId, request.StoreId,
            request.PurchaseOrderId, request.Notes);

        await _uow.SupplierReturns.AddAsync(supplierReturn, ct);
        await _uow.SaveChangesAsync(ct);

        // reload with navigation for mapper
        var loaded = await _uow.SupplierReturns.GetWithItemsAsync(tenantId, supplierReturn.Id, ct);
        return SupplierReturnMapper.ToResponse(loaded!);
    }
}
