using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Till;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Till.Queries;

// Get single session
public record GetTillSessionQuery(Guid Id) : IRequest<TillSessionResponse>;

public class GetTillSessionQueryHandler : IRequestHandler<GetTillSessionQuery, TillSessionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetTillSessionQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TillSessionResponse> Handle(GetTillSessionQuery request, CancellationToken ct)
    {
        var session = await _uow.TillSessions.GetWithPettyAsync(
            _currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("TillSession", request.Id);
        return TillMapper.ToResponse(session);
    }
}

// Get open session for terminal
public record GetOpenTillSessionQuery(Guid TerminalId) : IRequest<TillSessionResponse?>;

public class GetOpenTillSessionQueryHandler
    : IRequestHandler<GetOpenTillSessionQuery, TillSessionResponse?>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetOpenTillSessionQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TillSessionResponse?> Handle(
        GetOpenTillSessionQuery request, CancellationToken ct)
    {
        var session = await _uow.TillSessions.GetOpenSessionAsync(
            _currentUser.TenantId, request.TerminalId, ct);
        return session is null ? null : TillMapper.ToResponse(session);
    }
}

// List sessions
public record GetTillSessionsQuery(
    Guid? StoreId, Guid? TerminalId, bool OpenOnly, int Page, int PageSize)
    : IRequest<TillSessionListResponse>;

public class GetTillSessionsQueryHandler
    : IRequestHandler<GetTillSessionsQuery, TillSessionListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetTillSessionsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TillSessionListResponse> Handle(
        GetTillSessionsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var items = await _uow.TillSessions.GetPagedAsync(tenantId,
            request.StoreId, request.TerminalId, request.OpenOnly,
            request.Page, request.PageSize, ct);
        var total = await _uow.TillSessions.GetTotalCountAsync(tenantId,
            request.StoreId, request.TerminalId, request.OpenOnly, ct);
        return new TillSessionListResponse(
            items.Select(TillMapper.ToSummary).ToList().AsReadOnly(),
            total, request.Page, request.PageSize);
    }
}

// X-Report (mid-shift, no close)
public record GetXReportQuery(Guid SessionId) : IRequest<TillReportResponse>;

public class GetXReportQueryHandler : IRequestHandler<GetXReportQuery, TillReportResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetXReportQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TillReportResponse> Handle(GetXReportQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var session = await _uow.TillSessions.GetWithPettyAsync(tenantId, request.SessionId, ct)
            ?? throw new NotFoundException("TillSession", request.SessionId);

        var txns = await _uow.Transactions.GetByTerminalAsync(
            tenantId, session.TerminalId, session.OpenedAt, DateTime.UtcNow, ct);

        var completed = txns
            .Where(t => t.TransactionStatus == Domain.Enums.TransactionStatus.Completed).ToList();

        var cashSales = completed
            .Where(t => t.Type != Domain.Enums.TransactionType.Return)
            .SelectMany(t => t.Payments)
            .Where(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Approved
                && p.Method == Domain.Enums.PaymentMethod.Cash)
            .Sum(p => p.Amount);

        var cardSales = completed
            .Where(t => t.Type != Domain.Enums.TransactionType.Return)
            .SelectMany(t => t.Payments)
            .Where(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Approved
                && p.Method is Domain.Enums.PaymentMethod.Card
                    or Domain.Enums.PaymentMethod.BKash
                    or Domain.Enums.PaymentMethod.Nagad
                    or Domain.Enums.PaymentMethod.Rocket)
            .Sum(p => p.Amount);

        var giftCardSales = completed
            .Where(t => t.Type != Domain.Enums.TransactionType.Return)
            .SelectMany(t => t.Payments)
            .Where(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Approved
                && p.Method == Domain.Enums.PaymentMethod.GiftCard)
            .Sum(p => p.Amount);

        var refunds = completed
            .Where(t => t.Type == Domain.Enums.TransactionType.Return)
            .Sum(t => t.Total);

        var snap = session.TakeXReport(cashSales, cardSales, giftCardSales, refunds);
        return TillMapper.ToReport(snap, null);
    }
}
