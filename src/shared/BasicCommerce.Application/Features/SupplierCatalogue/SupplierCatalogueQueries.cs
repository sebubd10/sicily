using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Suppliers;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.SupplierCatalogue;

// ── Paginated catalogue for one supplier ──────────────────────
public record GetSupplierCatalogueQuery(Guid SupplierId, int Page, int PageSize)
    : IRequest<SupplierProductListResponse>;

public class GetSupplierCatalogueQueryHandler
    : IRequestHandler<GetSupplierCatalogueQuery, SupplierProductListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetSupplierCatalogueQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<SupplierProductListResponse> Handle(
        GetSupplierCatalogueQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var supplier = await _uow.Suppliers.GetByIdForTenantAsync(
            tenantId, request.SupplierId, ct)
            ?? throw new NotFoundException("Supplier", request.SupplierId);

        var items = await _uow.SupplierProducts.GetBySupplierPagedAsync(
            tenantId, request.SupplierId, request.Page, request.PageSize, ct);
        var total = await _uow.SupplierProducts.GetCountBySupplierAsync(
            tenantId, request.SupplierId, ct);

        var responses = new List<SupplierProductResponse>();
        foreach (var sp in items)
        {
            var product = await _uow.Products.GetByIdForTenantAsync(tenantId, sp.ProductId, ct);
            if (product is null) continue;
            responses.Add(SupplierCatalogueMapper.ToResponse(
                sp, supplier.Name, product.Name, product.Sku));
        }

        return new SupplierProductListResponse(
            responses.AsReadOnly(), total, request.Page, request.PageSize);
    }
}

// ── All suppliers that carry a given product (cheapest first) ─
public record GetProductSuppliersQuery(Guid ProductId)
    : IRequest<IReadOnlyList<SupplierProductResponse>>;

public class GetProductSuppliersQueryHandler
    : IRequestHandler<GetProductSuppliersQuery, IReadOnlyList<SupplierProductResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetProductSuppliersQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<IReadOnlyList<SupplierProductResponse>> Handle(
        GetProductSuppliersQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var product = await _uow.Products.GetByIdForTenantAsync(tenantId, request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        var entries = await _uow.SupplierProducts.GetByProductIdAsync(
            tenantId, request.ProductId, ct);

        var responses = new List<SupplierProductResponse>();
        foreach (var sp in entries)
        {
            var supplier = await _uow.Suppliers.GetByIdForTenantAsync(tenantId, sp.SupplierId, ct);
            if (supplier is null) continue;
            responses.Add(SupplierCatalogueMapper.ToResponse(
                sp, supplier.Name, product.Name, product.Sku));
        }

        return responses.AsReadOnly();
    }
}

// ── Single lookup for one supplier + product pair ─────────────
public record LookupSupplierPriceQuery(Guid SupplierId, Guid ProductId)
    : IRequest<SupplierProductResponse?>;

public class LookupSupplierPriceQueryHandler
    : IRequestHandler<LookupSupplierPriceQuery, SupplierProductResponse?>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public LookupSupplierPriceQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<SupplierProductResponse?> Handle(
        LookupSupplierPriceQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var sp = await _uow.SupplierProducts.GetBySupplierAndProductAsync(
            tenantId, request.SupplierId, request.ProductId, ct);
        if (sp is null) return null;

        var supplier = await _uow.Suppliers.GetByIdForTenantAsync(tenantId, sp.SupplierId, ct);
        var product = await _uow.Products.GetByIdForTenantAsync(tenantId, sp.ProductId, ct);
        if (supplier is null || product is null) return null;

        return SupplierCatalogueMapper.ToResponse(sp, supplier.Name, product.Name, product.Sku);
    }
}
