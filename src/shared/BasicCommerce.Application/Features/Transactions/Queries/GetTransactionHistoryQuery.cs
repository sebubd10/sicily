using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Queries;

public record GetTransactionHistoryQuery(Guid TenantId, Guid TransactionId)
    : IRequest<IReadOnlyList<TransactionAuditEntryResponse>>;

public class GetTransactionHistoryQueryHandler
    : IRequestHandler<GetTransactionHistoryQuery, IReadOnlyList<TransactionAuditEntryResponse>>
{
    private readonly IUnitOfWork _uow;

    public GetTransactionHistoryQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<TransactionAuditEntryResponse>> Handle(
        GetTransactionHistoryQuery request, CancellationToken ct)
    {
        var entries = await _uow.TransactionAudits.GetByTransactionAsync(
            request.TenantId, request.TransactionId, ct);

        var names = new Dictionary<Guid, string>();
        foreach (var userId in entries.Where(e => e.UserId.HasValue).Select(e => e.UserId!.Value).Distinct())
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct);
            if (user is not null) names[userId] = user.FullName;
        }

        return entries.Select(e => new TransactionAuditEntryResponse(
            e.Id, e.Action.ToString(), e.Summary, e.Reason, e.UserId,
            e.UserId is Guid id && names.TryGetValue(id, out var n) ? n : null,
            e.CreatedAt)).ToList();
    }
}
