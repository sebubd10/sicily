using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Suppliers.Commands;

public record DeleteSupplierCommand(Guid SupplierId) : IRequest;

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteSupplierCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteSupplierCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId, ct)
            ?? throw new NotFoundException("Supplier", request.SupplierId);
        if (supplier.TenantId != tenantId)
            throw new NotFoundException("Supplier", request.SupplierId);

        var productCount = await _uow.SupplierProducts.GetCountBySupplierAsync(tenantId, supplier.Id, ct);
        if (productCount > 0)
            throw new DomainException(
                $"Cannot delete: {productCount} product{(productCount == 1 ? " is" : "s are")} linked to this supplier. Remove the links first.");

        supplier.SoftDelete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
