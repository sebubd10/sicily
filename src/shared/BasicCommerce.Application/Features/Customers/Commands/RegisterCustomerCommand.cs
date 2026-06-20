using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Customers;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Commands;

public record RegisterCustomerCommand(
    string Name,
    string? Email,
    string? Phone,
    decimal CreditLimit,
    string? AddressLine1,
    string? City,
    string? District,
    string? PostalCode) : IRequest<CustomerResponse>;

public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
    }
}

public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, CustomerResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RegisterCustomerCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CustomerResponse> Handle(RegisterCustomerCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        if (!string.IsNullOrWhiteSpace(request.Phone))
            if (await _uow.Customers.GetByPhoneAsync(tenantId, request.Phone, ct) is not null)
                throw new DomainException($"Phone '{request.Phone}' is already registered.");

        if (!string.IsNullOrWhiteSpace(request.Email))
            if (await _uow.Customers.GetByEmailAsync(tenantId, request.Email, ct) is not null)
                throw new DomainException($"Email '{request.Email}' is already registered.");

        Address? address = null;
        if (!string.IsNullOrWhiteSpace(request.AddressLine1) &&
            !string.IsNullOrWhiteSpace(request.City))
        {
            address = new Address(
                request.AddressLine1,
                null,
                request.City,
                request.District ?? string.Empty,
                request.PostalCode ?? string.Empty);
        }

        var customer = Customer.Create(tenantId, request.Name, request.Email,
            request.Phone, request.CreditLimit);

        if (address is not null)
            customer.Update(customer.Name, customer.Email, customer.Phone, address);

        await _uow.Customers.AddAsync(customer, ct);

        if (request.CreditLimit > 0)
        {
            if (_currentUser.StoreId.HasValue)
            {
                var creditAccount = CreditAccount.Create(tenantId, customer.Id,
                    _currentUser.StoreId.Value, request.CreditLimit);
                await _uow.CreditAccounts.AddAsync(creditAccount, ct);
            }
            else
            {
                // Global admin (no store context): create an account per active store
                var stores = await _uow.Stores.FindAsync(
                    s => s.TenantId == tenantId && s.Status == EntityStatus.Active, ct);
                foreach (var store in stores)
                    await _uow.CreditAccounts.AddAsync(
                        CreditAccount.Create(tenantId, customer.Id, store.Id, request.CreditLimit), ct);
            }
        }

        var rewardSettings = await _uow.RewardPointsSettings.GetByTenantAsync(tenantId, ct);
        if (rewardSettings is { Status: EntityStatus.Active, PointsForRegistration: > 0 })
        {
            var storeId = rewardSettings.PointsAccumulatedForAllStores
                ? null : _currentUser.StoreId;

            var rewardAccount = RewardPointsAccount.Create(tenantId, customer.Id, storeId);
            await _uow.RewardPointsAccounts.AddAsync(rewardAccount, ct);

            rewardAccount.EarnPoints(
                rewardSettings.PointsForRegistration,
                RewardPointsEntryType.RegistrationEarned,
                rewardSettings.ActivatePointsImmediately,
                rewardSettings.RegistrationPointsValidityDays,
                notes: "Registration bonus");

            customer.AddLoyaltyPoints(rewardSettings.PointsForRegistration);
        }

        await _uow.SaveChangesAsync(ct);
        return ToResponse(customer);
    }

    internal static CustomerResponse ToResponse(Customer c) => new(
        c.Id, c.Code, c.Name, c.Email, c.Phone,
        c.Address?.Line1, c.Address?.City,
        c.LoyaltyPoints, c.CreditLimit, c.CurrentBalance, c.AvailableCredit,
        c.Status.ToString(), c.CreatedAt);
}
