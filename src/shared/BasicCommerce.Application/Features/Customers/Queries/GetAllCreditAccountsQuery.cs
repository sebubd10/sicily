using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Customers;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Queries;

public record GetAllCreditAccountsQuery(
    string? Term,
    bool? HasBalance,
    int Page = 1,
    int PageSize = 20) : IRequest<CreditAccountListResponse>;

public class GetAllCreditAccountsQueryHandler
    : IRequestHandler<GetAllCreditAccountsQuery, CreditAccountListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetAllCreditAccountsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CreditAccountListResponse> Handle(
        GetAllCreditAccountsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var (items, totalCount, totalOutstanding, totalCredit) =
            await _uow.CreditAccounts.GetPagedAsync(tenantId, request.Term, request.HasBalance, page, size, ct);

        var accountList = items.ToList();

        var customerIds = accountList.Select(a => a.CustomerId).Distinct().ToList();
        var storeIds    = accountList.Select(a => a.StoreId).Distinct().ToList();

        var customers = new Dictionary<Guid, (string Name, string Code)>();
        foreach (var id in customerIds)
        {
            var c = await _uow.Customers.GetByIdAsync(id, ct);
            if (c is not null) customers[id] = (c.Name, c.Code);
        }

        var stores = new Dictionary<Guid, string>();
        foreach (var id in storeIds)
        {
            var s = await _uow.Stores.GetByIdAsync(id, ct);
            if (s is not null) stores[id] = s.Name;
        }

        var summaries = accountList.Select(a =>
        {
            var (name, code) = customers.GetValueOrDefault(a.CustomerId, ("Unknown", "—"));
            var storeName    = stores.GetValueOrDefault(a.StoreId, "—");
            return new CreditAccountSummary(
                a.Id, a.CustomerId, name, code, storeName,
                a.CreditLimit, a.OutstandingBalance, a.AvailableCredit,
                a.Status.ToString(), a.LastPaymentAt, a.CreatedAt);
        });

        return new CreditAccountListResponse(summaries, totalCount, page, size, totalOutstanding, totalCredit);
    }
}
