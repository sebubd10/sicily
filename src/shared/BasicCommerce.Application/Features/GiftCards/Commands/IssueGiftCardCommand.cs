using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.GiftCards;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.GiftCards.Commands;

public record IssueGiftCardCommand(
    Guid StoreId,
    decimal Amount,
    DateTime? ExpiryDate,
    Guid? IssuedToCustomerId,
    Guid? IssuedInTransactionId,
    string? Notes) : IRequest<GiftCardResponse>;

public class IssueGiftCardCommandValidator : AbstractValidator<IssueGiftCardCommand>
{
    public IssueGiftCardCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
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
            await _uow.GiftCards.GetWithTransactionsAsync(tenantId, card.Id, ct) ?? card);
    }

    private static string GenerateCode()
        => $"GC-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
}
