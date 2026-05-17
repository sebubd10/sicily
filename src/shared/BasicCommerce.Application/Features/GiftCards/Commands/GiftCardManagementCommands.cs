using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.GiftCards;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.GiftCards.Commands;

// Reload
public record ReloadGiftCardCommand(Guid Id, decimal Amount, string? Notes)
    : IRequest<GiftCardResponse>;

public class ReloadGiftCardCommandValidator : AbstractValidator<ReloadGiftCardCommand>
{
    public ReloadGiftCardCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class ReloadGiftCardCommandHandler : IRequestHandler<ReloadGiftCardCommand, GiftCardResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ReloadGiftCardCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<GiftCardResponse> Handle(ReloadGiftCardCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var card = await _uow.GiftCards.GetWithTransactionsAsync(tenantId, request.Id, ct)
            ?? throw new NotFoundException("GiftCard", request.Id);

        card.Reload(request.Amount, request.Notes);
        _uow.GiftCards.Update(card);
        await _uow.SaveChangesAsync(ct);
        return GiftCardMapper.ToResponse(card);
    }
}

// Cancel
public record CancelGiftCardCommand(Guid Id, string? Reason) : IRequest<GiftCardResponse>;

public class CancelGiftCardCommandHandler : IRequestHandler<CancelGiftCardCommand, GiftCardResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CancelGiftCardCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<GiftCardResponse> Handle(CancelGiftCardCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var card = await _uow.GiftCards.GetWithTransactionsAsync(tenantId, request.Id, ct)
            ?? throw new NotFoundException("GiftCard", request.Id);

        card.Cancel(request.Reason);
        _uow.GiftCards.Update(card);
        await _uow.SaveChangesAsync(ct);
        return GiftCardMapper.ToResponse(card);
    }
}
