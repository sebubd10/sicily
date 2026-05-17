using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Suppliers;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Suppliers.Queries;

public record GetSuppliersQuery : IRequest<IEnumerable<SupplierResponse>>;
public record GetSupplierQuery(Guid SupplierId) : IRequest<SupplierResponse>;

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, IEnumerable<SupplierResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetSuppliersQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<SupplierResponse>> Handle(GetSuppliersQuery request, CancellationToken ct)
    {
        var all = await _uow.Suppliers.GetAllForTenantAsync(_currentUser.TenantId, ct);
        return all.Select(SupplierMapper.ToResponse);
    }
}

public class GetSupplierQueryHandler : IRequestHandler<GetSupplierQuery, SupplierResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetSupplierQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierResponse> Handle(GetSupplierQuery request, CancellationToken ct)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId, ct)
            ?? throw new NotFoundException("Supplier", request.SupplierId);
        if (supplier.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Supplier", request.SupplierId);
        return SupplierMapper.ToResponse(supplier);
    }
}
