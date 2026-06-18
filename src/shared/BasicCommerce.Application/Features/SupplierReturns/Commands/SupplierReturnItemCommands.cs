using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.SupplierReturns;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.SupplierReturns.Commands;

// ── Add item ─────────────────────────────────────────────────────────────────

public record AddSupplierReturnItemCommand(
    Guid SupplierReturnId,
    Guid ProductId,
    decimal Quantity,
    decimal UnitCost,
    SupplierReturnReason Reason,
    string? Notes) : IRequest<SupplierReturnResponse>;

public class AddSupplierReturnItemCommandValidator
    : AbstractValidator<AddSupplierReturnItemCommand>
{
    public AddSupplierReturnItemCommandValidator()
    {
        RuleFor(x => x.SupplierReturnId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
    }
}

public class AddSupplierReturnItemCommandHandler
    : IRequestHandler<AddSupplierReturnItemCommand, SupplierReturnResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AddSupplierReturnItemCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierReturnResponse> Handle(
        AddSupplierReturnItemCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var sr = await _uow.SupplierReturns.GetWithItemsAsync(tenantId, request.SupplierReturnId, ct)
            ?? throw new NotFoundException("SupplierReturn", request.SupplierReturnId);

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);
        if (product.TenantId != tenantId) throw new NotFoundException("Product", request.ProductId);
        if (product.Status != EntityStatus.Active)
            throw new DomainException($"Cannot add '{product.Name}' to the return because the product is not active.");

        var item = sr.AddItem(request.ProductId, request.Quantity, request.UnitCost, request.Reason, request.Notes);
        _uow.SupplierReturns.AddItem(item);
        await _uow.SaveChangesAsync(ct);

        var loaded = await _uow.SupplierReturns.GetWithItemsAsync(tenantId, sr.Id, ct);
        return SupplierReturnMapper.ToResponse(loaded!);
    }
}

// ── Remove item ───────────────────────────────────────────────────────────────

public record RemoveSupplierReturnItemCommand(
    Guid SupplierReturnId, Guid ProductId) : IRequest<SupplierReturnResponse>;

public class RemoveSupplierReturnItemCommandHandler
    : IRequestHandler<RemoveSupplierReturnItemCommand, SupplierReturnResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RemoveSupplierReturnItemCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierReturnResponse> Handle(
        RemoveSupplierReturnItemCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var sr = await _uow.SupplierReturns.GetWithItemsAsync(tenantId, request.SupplierReturnId, ct)
            ?? throw new NotFoundException("SupplierReturn", request.SupplierReturnId);

        var item = sr.Items.FirstOrDefault(i => i.ProductId == request.ProductId)
            ?? throw new NotFoundException("SupplierReturnItem", request.ProductId);
        sr.RemoveItem(request.ProductId);
        _uow.SupplierReturns.RemoveItem(item);
        await _uow.SaveChangesAsync(ct);

        var loaded = await _uow.SupplierReturns.GetWithItemsAsync(tenantId, sr.Id, ct);
        return SupplierReturnMapper.ToResponse(loaded!);
    }
}
