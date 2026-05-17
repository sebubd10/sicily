using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Customers;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Queries;

public record GetCreditAccountQuery(Guid CreditAccountId) : IRequest<CreditAccountResponse>;
public record GetCustomerCreditAccountsQuery(Guid CustomerId) : IRequest<IEnumerable<CreditAccountResponse>>;

public class GetCreditAccountQueryHandler : IRequestHandler<GetCreditAccountQuery, CreditAccountResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetCreditAccountQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CreditAccountResponse> Handle(GetCreditAccountQuery request, CancellationToken ct)
    {
        var account = await _uow.CreditAccounts.GetWithTransactionsAsync(
            _currentUser.TenantId, request.CreditAccountId, ct)
            ?? throw new NotFoundException("CreditAccount", request.CreditAccountId);

        var customer = await _uow.Customers.GetByIdAsync(account.CustomerId, ct);
        var store = await _uow.Stores.GetByIdAsync(account.StoreId, ct);

        return ToResponse(account, customer?.Name ?? "Unknown",
            customer?.Code ?? string.Empty, store?.Name ?? string.Empty);
    }

    internal static CreditAccountResponse ToResponse(CreditAccount account,
        string customerName, string customerCode, string storeName) =>
        new(account.Id,
            account.CustomerId,
            customerName,
            customerCode,
            storeName,
            account.CreditLimit,
            account.OutstandingBalance,
            account.AvailableCredit,
            account.Status.ToString(),
            account.LastPaymentAt,
            account.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Take(50)
                .Select(t => new CreditTransactionRow(
                    t.Id,
                    t.IsCharge ? "Charge" : "Payment",
                    t.Amount,
                    t.Description,
                    t.Reference,
                    t.CreatedAt)));
}

public class GetCustomerCreditAccountsQueryHandler
    : IRequestHandler<GetCustomerCreditAccountsQuery, IEnumerable<CreditAccountResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetCustomerCreditAccountsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<CreditAccountResponse>> Handle(
        GetCustomerCreditAccountsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);
        if (customer.TenantId != tenantId)
            throw new NotFoundException("Customer", request.CustomerId);

        var accounts = await _uow.CreditAccounts.GetByCustomerAsync(tenantId, request.CustomerId, ct);

        var result = new List<CreditAccountResponse>();
        foreach (var account in accounts)
        {
            var store = await _uow.Stores.GetByIdAsync(account.StoreId, ct);
            var full = await _uow.CreditAccounts.GetWithTransactionsAsync(tenantId, account.Id, ct);
            if (full is null) continue;
            result.Add(GetCreditAccountQueryHandler.ToResponse(
                full, customer.Name, customer.Code, store?.Name ?? string.Empty));
        }
        return result;
    }
}
