using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.GiftCards;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.GiftCards.Queries;

// Get by ID
public record GetGiftCardQuery(Guid Id) : IRequest<GiftCardResponse>;

public class GetGiftCardQueryHandler : IRequestHandler<GetGiftCardQuery, GiftCardResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetGiftCardQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<GiftCardResponse> Handle(GetGiftCardQuery request, CancellationToken ct)
    {
        var card = await _uow.GiftCards.GetWithTransactionsAsync(
            _currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("GiftCard", request.Id);
        return GiftCardMapper.ToResponse(card);
    }
}

// Check balance by code
public record CheckGiftCardBalanceQuery(string Code) : IRequest<CheckBalanceResponse>;

public class CheckGiftCardBalanceQueryHandler
    : IRequestHandler<CheckGiftCardBalanceQuery, CheckBalanceResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CheckGiftCardBalanceQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CheckBalanceResponse> Handle(
        CheckGiftCardBalanceQuery request, CancellationToken ct)
    {
        var card = await _uow.GiftCards.GetByCodeAsync(
            _currentUser.TenantId, request.Code, ct)
            ?? throw new NotFoundException("GiftCard", request.Code);
        return new CheckBalanceResponse(card.Code, card.Balance,
            card.CardStatus.ToString(), card.ExpiryDate);
    }
}

// List paged
public record GetGiftCardsQuery(
    Guid? StoreId, string? Status, int Page, int PageSize)
    : IRequest<GiftCardListResponse>;

public class GetGiftCardsQueryHandler : IRequestHandler<GetGiftCardsQuery, GiftCardListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetGiftCardsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<GiftCardListResponse> Handle(
        GetGiftCardsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var items = await _uow.GiftCards.GetPagedAsync(
            tenantId, request.StoreId, request.Status, request.Page, request.PageSize, ct);
        var total = await _uow.GiftCards.GetTotalCountAsync(
            tenantId, request.StoreId, request.Status, ct);
        return new GiftCardListResponse(
            items.Select(GiftCardMapper.ToSummary).ToList().AsReadOnly(),
            total, request.Page, request.PageSize);
    }
}
