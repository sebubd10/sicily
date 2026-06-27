using BasicCommerce.Application.Features.Transactions.Commands;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Queries;

public record SearchTransactionsQuery(
    Guid TenantId,
    string? Term,
    Guid? StoreId,
    string? Status,
    string? Type,
    DateTime? From,
    DateTime? To,
    Guid? CustomerId,
    int Page = 1,
    int PageSize = 20) : IRequest<TransactionListResponse>;

public class SearchTransactionsQueryHandler
    : IRequestHandler<SearchTransactionsQuery, TransactionListResponse>
{
    private readonly IUnitOfWork _uow;

    public SearchTransactionsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionListResponse> Handle(
        SearchTransactionsQuery request, CancellationToken ct)
    {
        TransactionStatus? status = Enum.TryParse<TransactionStatus>(request.Status, ignoreCase: true, out var s) ? s : null;
        TransactionType? type = Enum.TryParse<TransactionType>(request.Type, ignoreCase: true, out var ty) ? ty : null;

        var (transactions, total) = await FetchAsync(request, status, type, ct);

        var customerIds = transactions
            .Where(t => t.CustomerId.HasValue)
            .Select(t => t.CustomerId!.Value)
            .Distinct().ToList();

        var customerMap = new Dictionary<Guid, string>();
        foreach (var cid in customerIds)
        {
            var c = await _uow.Customers.GetByIdAsync(cid, ct);
            if (c is not null) customerMap[cid] = c.Name;
        }

        var storeIds = transactions.Select(t => t.StoreId).Distinct().ToList();
        var storeMap = new Dictionary<Guid, string>();
        foreach (var sid in storeIds)
        {
            var store = await _uow.Stores.GetByIdAsync(sid, ct);
            if (store is not null) storeMap[sid] = store.Name;
        }

        var summaries = transactions.Select(t => new TransactionSummary(
            Id: t.Id,
            TransactionNumber: t.TransactionNumber,
            Status: t.TransactionStatus.ToString(),
            Type: t.Type.ToString(),
            StoreId: t.StoreId,
            StoreName: storeMap.TryGetValue(t.StoreId, out var sn) ? sn : null,
            CustomerId: t.CustomerId,
            CustomerName: t.CustomerId.HasValue && customerMap.TryGetValue(t.CustomerId.Value, out var cn) ? cn : null,
            Total: t.Total,
            AmountPaid: t.AmountPaid,
            ItemCount: t.LineItems.Count(l => !l.IsVoided),
            CreatedAt: t.CreatedAt,
            CompletedAt: t.CompletedAt));

        return new TransactionListResponse(summaries, total, request.Page, request.PageSize);
    }

    private async Task<(IEnumerable<Domain.Entities.Transaction> Items, int Total)> FetchAsync(
        SearchTransactionsQuery request,
        TransactionStatus? status,
        TransactionType? type,
        CancellationToken ct)
    {
        var items = await _uow.Transactions.SearchPagedAsync(
            request.TenantId, request.Term, request.StoreId,
            status, type, request.From, request.To, request.CustomerId,
            request.Page, request.PageSize, ct);

        var total = await _uow.Transactions.SearchCountAsync(
            request.TenantId, request.Term, request.StoreId,
            status, type, request.From, request.To, request.CustomerId, ct);

        return (items, total);
    }
}
