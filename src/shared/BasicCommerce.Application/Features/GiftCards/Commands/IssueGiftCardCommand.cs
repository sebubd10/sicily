using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.GiftCards;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.GiftCards.Commands;

/// <param name="StoreId">Restrict the card to one store, or null to allow every store.</param>
/// <param name="IssuedToCustomerId">Restrict the card to one customer, or null so anyone can use it.</param>
public record IssueGiftCardCommand(
    Guid? StoreId,
    decimal Amount,
    DateTime? ExpiryDate,
    Guid? IssuedToCustomerId,
    Guid? IssuedInTransactionId,
    string? Notes) : IRequest<GiftCardResponse>;

public class IssueGiftCardCommandValidator : AbstractValidator<IssueGiftCardCommand>
{
    public IssueGiftCardCommandValidator()
    {
        RuleFor(x => x.IssuedToCustomerId).NotEqual(Guid.Empty).When(x => x.IssuedToCustomerId.HasValue);
        RuleFor(x => x.StoreId).NotEqual(Guid.Empty).When(x => x.StoreId.HasValue);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ExpiryDate).GreaterThan(DateTime.UtcNow.Date)
            .When(x => x.ExpiryDate.HasValue)
            .WithMessage("Expiry date must be in the future.");
    }
}

public class IssueGiftCardCommandHandler : IRequestHandler<IssueGiftCardCommand, GiftCardResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public IssueGiftCardCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<GiftCardResponse> Handle(IssueGiftCardCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        Customer? customer = null;
        if (request.IssuedToCustomerId is Guid customerId)
        {
            customer = await _uow.Customers.GetByIdForTenantAsync(tenantId, customerId, ct)
                ?? throw new NotFoundException("Customer", customerId);
            if (customer.Status != EntityStatus.Active)
                throw new DomainException($"Customer '{customer.Name}' is inactive.");
        }

        if (request.StoreId is Guid storeId)
        {
            var store = await _uow.Stores.GetByIdForTenantAsync(tenantId, storeId, ct)
                ?? throw new NotFoundException("Store", storeId);
            if (store.Status != EntityStatus.Active)
                throw new DomainException($"Store '{store.Name}' is inactive.");
        }

        var code = GenerateCode();

        // Ensure uniqueness
        while (await _uow.GiftCards.GetByCodeAsync(tenantId, code, ct) is not null)
            code = GenerateCode();

        var card = GiftCard.Issue(tenantId, request.StoreId, code, request.Amount,
            request.ExpiryDate, request.IssuedToCustomerId,
            request.IssuedInTransactionId, request.Notes);

        await _uow.GiftCards.AddAsync(card, ct);
        await _uow.SaveChangesAsync(ct);

        return GiftCardMapper.ToResponse(
            await _uow.GiftCards.GetWithTransactionsAsync(tenantId, card.Id, ct) ?? card,
            customer?.Name);
    }

    private static string GenerateCode()
        => $"GC-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
}
