using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Suppliers;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.SupplierCatalogue;

// ── Upsert (create or update) a catalogue entry ───────────────
public record UpsertSupplierProductCommand(
    Guid SupplierId,
    Guid ProductId,
    decimal UnitCost,
    string CurrencyCode,
    string? SupplierSku,
    int? MinOrderQuantity,
    int? LeadTimeDays,
    string? Notes) : IRequest<SupplierProductResponse>;

public class UpsertSupplierProductCommandValidator
    : AbstractValidator<UpsertSupplierProductCommand>
{
    public UpsertSupplierProductCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrencyCode).NotEmpty().MaximumLength(3);
    }
}

public class UpsertSupplierProductCommandHandler
    : IRequestHandler<UpsertSupplierProductCommand, SupplierProductResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpsertSupplierProductCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<SupplierProductResponse> Handle(
        UpsertSupplierProductCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var supplier = await _uow.Suppliers.GetByIdForTenantAsync(tenantId, request.SupplierId, ct)
            ?? throw new NotFoundException("Supplier", request.SupplierId);

        var product = await _uow.Products.GetByIdForTenantAsync(tenantId, request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        var existing = await _uow.SupplierProducts.GetBySupplierAndProductAsync(
            tenantId, request.SupplierId, request.ProductId, ct);

        if (existing is null)
        {
            existing = SupplierProduct.Create(tenantId, request.SupplierId, request.ProductId,
                request.UnitCost, request.CurrencyCode, request.SupplierSku,
                request.MinOrderQuantity, request.LeadTimeDays, request.Notes);
            await _uow.SupplierProducts.AddAsync(existing, ct);
        }
        else
        {
            existing.Update(request.UnitCost, request.SupplierSku,
                request.MinOrderQuantity, request.LeadTimeDays, request.Notes, request.CurrencyCode);
            _uow.SupplierProducts.Update(existing);
        }

        await _uow.SaveChangesAsync(ct);

        return SupplierCatalogueMapper.ToResponse(existing, supplier.Name,
            product.Name, product.Sku);
    }
}

// ── Delete a catalogue entry ───────────────────────────────────
public record DeleteSupplierProductCommand(Guid SupplierId, Guid ProductId) : IRequest;

public class DeleteSupplierProductCommandHandler
    : IRequestHandler<DeleteSupplierProductCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteSupplierProductCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteSupplierProductCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var entry = await _uow.SupplierProducts.GetBySupplierAndProductAsync(
            tenantId, request.SupplierId, request.ProductId, ct)
            ?? throw new NotFoundException("SupplierProduct",
                $"{request.SupplierId}/{request.ProductId}");

        entry.Status = Domain.Enums.EntityStatus.Deleted;
        _uow.SupplierProducts.Update(entry);
        await _uow.SaveChangesAsync(ct);
    }
}
