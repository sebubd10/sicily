using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.SupplierReturns;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.SupplierReturns.Commands;

// ── Submit ────────────────────────────────────────────────────────────────────

public record SubmitSupplierReturnCommand(Guid Id) : IRequest<SupplierReturnResponse>;

public class SubmitSupplierReturnCommandHandler
    : IRequestHandler<SubmitSupplierReturnCommand, SupplierReturnResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SubmitSupplierReturnCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierReturnResponse> Handle(
        SubmitSupplierReturnCommand request, CancellationToken ct)
    {
        var sr = await _uow.SupplierReturns.GetWithItemsAsync(_currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("SupplierReturn", request.Id);

        sr.Submit();
        _uow.SupplierReturns.Update(sr);
        await _uow.SaveChangesAsync(ct);
        return SupplierReturnMapper.ToResponse(sr);
    }
}

// ── Ship (decrements stock) ───────────────────────────────────────────────────

public record ShipSupplierReturnCommand(Guid Id) : IRequest<SupplierReturnResponse>;

public class ShipSupplierReturnCommandHandler
    : IRequestHandler<ShipSupplierReturnCommand, SupplierReturnResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ShipSupplierReturnCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierReturnResponse> Handle(
        ShipSupplierReturnCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var sr = await _uow.SupplierReturns.GetWithItemsAsync(tenantId, request.Id, ct)
            ?? throw new NotFoundException("SupplierReturn", request.Id);

        foreach (var item in sr.Items)
        {
            var stockLevel = await _uow.StockLevels.GetAsync(
                tenantId, sr.StoreId, item.ProductId, ct);

            if (stockLevel is null) continue;

            var before = stockLevel.Quantity;
            if (stockLevel.AvailableQuantity >= item.Quantity)
            {
                stockLevel.Decrement(item.Quantity);
                _uow.StockLevels.Update(stockLevel);
            }

            await _uow.StockMovements.AddAsync(StockMovement.Create(
                tenantId, sr.StoreId, item.ProductId,
                StockMovementType.SupplierReturn, -item.Quantity, before,
                _currentUser.UserId,
                reference: sr.ReturnNumber,
                notes: $"Supplier return ({item.Reason}): {item.Product?.Name}"), ct);
        }

        sr.MarkShipped();
        _uow.SupplierReturns.Update(sr);
        await _uow.SaveChangesAsync(ct);
        return SupplierReturnMapper.ToResponse(sr);
    }
}

// ── Receive Credit ────────────────────────────────────────────────────────────

public record ReceiveCreditCommand(
    Guid Id, decimal CreditAmount, string? CreditNoteReference)
    : IRequest<SupplierReturnResponse>;

public class ReceiveCreditCommandValidator : AbstractValidator<ReceiveCreditCommand>
{
    public ReceiveCreditCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CreditAmount).GreaterThanOrEqualTo(0);
    }
}

public class ReceiveCreditCommandHandler
    : IRequestHandler<ReceiveCreditCommand, SupplierReturnResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ReceiveCreditCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierReturnResponse> Handle(
        ReceiveCreditCommand request, CancellationToken ct)
    {
        var sr = await _uow.SupplierReturns.GetWithItemsAsync(_currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("SupplierReturn", request.Id);

        sr.ReceiveCredit(request.CreditAmount, request.CreditNoteReference);
        _uow.SupplierReturns.Update(sr);
        await _uow.SaveChangesAsync(ct);
        return SupplierReturnMapper.ToResponse(sr);
    }
}

// ── Set Expected Credit ───────────────────────────────────────────────────────

public record SetExpectedCreditCommand(Guid Id, decimal ExpectedCreditAmount)
    : IRequest<SupplierReturnResponse>;

public class SetExpectedCreditCommandHandler
    : IRequestHandler<SetExpectedCreditCommand, SupplierReturnResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SetExpectedCreditCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierReturnResponse> Handle(
        SetExpectedCreditCommand request, CancellationToken ct)
    {
        var sr = await _uow.SupplierReturns.GetWithItemsAsync(_currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("SupplierReturn", request.Id);

        sr.SetExpectedCredit(request.ExpectedCreditAmount);
        _uow.SupplierReturns.Update(sr);
        await _uow.SaveChangesAsync(ct);
        return SupplierReturnMapper.ToResponse(sr);
    }
}

// ── Cancel ────────────────────────────────────────────────────────────────────

public record CancelSupplierReturnCommand(Guid Id) : IRequest<SupplierReturnResponse>;

public class CancelSupplierReturnCommandHandler
    : IRequestHandler<CancelSupplierReturnCommand, SupplierReturnResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CancelSupplierReturnCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierReturnResponse> Handle(
        CancelSupplierReturnCommand request, CancellationToken ct)
    {
        var sr = await _uow.SupplierReturns.GetWithItemsAsync(_currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("SupplierReturn", request.Id);

        sr.Cancel();
        _uow.SupplierReturns.Update(sr);
        await _uow.SaveChangesAsync(ct);
        return SupplierReturnMapper.ToResponse(sr);
    }
}
