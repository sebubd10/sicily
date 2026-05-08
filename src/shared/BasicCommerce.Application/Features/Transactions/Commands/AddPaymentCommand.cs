using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record AddPaymentCommand(
    Guid TenantId,
    Guid TransactionId,
    PaymentMethod Method,
    decimal Amount,
    string? MobileNumber = null,
    string? Reference = null,
    string? GiftCardCode = null) : IRequest<TransactionResponse>;

public class AddPaymentCommandValidator : AbstractValidator<AddPaymentCommand>
{
    public AddPaymentCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.MobileNumber).NotEmpty()
            .When(x => x.Method is PaymentMethod.BKash or PaymentMethod.Nagad or PaymentMethod.Rocket)
            .WithMessage("Mobile number is required for mobile wallet payments.");
        RuleFor(x => x.GiftCardCode).NotEmpty()
            .When(x => x.Method == PaymentMethod.GiftCard)
            .WithMessage("Gift card code is required for gift card payments.");
    }
}

public class AddPaymentCommandHandler : IRequestHandler<AddPaymentCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;

    public AddPaymentCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(AddPaymentCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        var payment = transaction.AddPayment(request.Method, request.Amount, request.Reference);

        if (request.MobileNumber is not null)
            payment.SetMobileNumber(request.MobileNumber);

        switch (request.Method)
        {
            case PaymentMethod.Cash:
                payment.Approve();
                break;

            case PaymentMethod.Credit:
                await ChargeCreditAccountAsync(transaction, request, ct);
                payment.Approve();
                break;

            case PaymentMethod.RewardPoints:
                await RedeemRewardPointsAsync(transaction, request, ct);
                payment.Approve();
                break;

            case PaymentMethod.GiftCard:
                await RedeemGiftCardAsync(transaction, request, payment, ct);
                payment.Approve();
                break;

            // Card / mobile wallet: mark pending — external gateway approves
        }

        transaction.RefreshAmountPaid();
        _uow.Transactions.Update(transaction);
        await _uow.SaveChangesAsync(ct);

        string? customerName = null;
        if (transaction.CustomerId.HasValue)
        {
            var customer = await _uow.Customers.GetByIdAsync(transaction.CustomerId.Value, ct);
            customerName = customer?.Name;
        }

        return CreateTransactionCommandHandler.MapToResponse(transaction, customerName);
    }

    private async Task RedeemRewardPointsAsync(
        Domain.Entities.Transaction transaction,
        AddPaymentCommand request, CancellationToken ct)
    {
        if (!transaction.CustomerId.HasValue)
            throw new DomainException("A customer must be attached to use reward points.");

        var settings = await _uow.RewardPointsSettings.GetByTenantAsync(request.TenantId, ct)
            ?? throw new DomainException("Reward points programme is not configured.");

        if (settings.Status != EntityStatus.Active)
            throw new DomainException("Reward points programme is not enabled.");

        var storeId = settings.PointsAccumulatedForAllStores ? null : transaction.StoreId;

        var account = await _uow.RewardPointsAccounts.GetByCustomerAsync(
            request.TenantId, transaction.CustomerId.Value, storeId, ct)
            ?? throw new DomainException("Customer has no reward points account.");

        // Convert currency amount to points
        if (settings.ExchangeRate <= 0)
            throw new DomainException("Invalid reward points exchange rate.");

        var pointsToRedeem = (int)Math.Ceiling(request.Amount / settings.ExchangeRate);

        var maxRedeemable = settings.CalculateMaxRedeemablePoints(
            account.AvailablePoints, transaction.Total);

        if (pointsToRedeem > maxRedeemable)
            throw new DomainException(
                $"Cannot redeem {pointsToRedeem} points. Maximum redeemable: {maxRedeemable}.");

        account.RedeemPoints(pointsToRedeem, transaction.Id,
            $"Redemption: {transaction.TransactionNumber}");

        var customer = await _uow.Customers.GetByIdAsync(transaction.CustomerId.Value, ct);
        customer?.RedeemLoyaltyPoints(pointsToRedeem);
        if (customer is not null) _uow.Customers.Update(customer);

        _uow.RewardPointsAccounts.Update(account);
    }

    private async Task RedeemGiftCardAsync(
        Domain.Entities.Transaction transaction,
        AddPaymentCommand request,
        Domain.Entities.Payment payment,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.GiftCardCode))
            throw new DomainException("Gift card code is required.");

        var card = await _uow.GiftCards.GetByCodeAsync(request.TenantId, request.GiftCardCode, ct)
            ?? throw new DomainException($"Gift card '{request.GiftCardCode}' not found.");

        if (card.Balance < request.Amount)
            throw new DomainException(
                $"Gift card balance ({card.Balance:F2}) is insufficient for this payment ({request.Amount:F2}). " +
                $"Use a smaller amount or split the payment.");

        var actual = card.Redeem(request.Amount, transaction.Id);
        payment.SetGiftCardRedemption(request.GiftCardCode, actual);
        _uow.GiftCards.Update(card);
    }

    private async Task ChargeCreditAccountAsync(
        Domain.Entities.Transaction transaction,
        AddPaymentCommand request, CancellationToken ct)
    {
        if (!transaction.CustomerId.HasValue)
            throw new DomainException("A customer must be attached for credit sales.");

        var creditAccount = await _uow.CreditAccounts
            .GetByCustomerAndStoreAsync(request.TenantId,
                transaction.CustomerId.Value, transaction.StoreId, ct)
            ?? throw new DomainException(
                "Customer has no credit account at this store.");

        creditAccount.Charge(transaction.Id, request.Amount,
            $"Sale: {transaction.TransactionNumber}");

        var customer = await _uow.Customers.GetByIdAsync(transaction.CustomerId.Value, ct);
        customer?.AddToBalance(request.Amount);
    }
}
