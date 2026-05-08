using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Promotions;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Promotions.Queries;

public record GetPromotionQuery(Guid Id) : IRequest<PromotionResponse>;

public class GetPromotionQueryHandler : IRequestHandler<GetPromotionQuery, PromotionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    public GetPromotionQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<PromotionResponse> Handle(GetPromotionQuery request, CancellationToken ct)
    {
        var p = await _uow.Promotions.GetByIdForTenantAsync(_currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("Promotion", request.Id);
        return PromotionMapper.ToResponse(p);
    }
}

public record GetPromotionsQuery(PromotionStatus? Status, int Page, int PageSize)
    : IRequest<PromotionListResponse>;

public class GetPromotionsQueryHandler : IRequestHandler<GetPromotionsQuery, PromotionListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    public GetPromotionsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<PromotionListResponse> Handle(GetPromotionsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var items = await _uow.Promotions.GetPagedAsync(tenantId, request.Status, request.Page, request.PageSize, ct);
        var total = await _uow.Promotions.GetTotalCountAsync(tenantId, request.Status, ct);
        return new PromotionListResponse(
            items.Select(PromotionMapper.ToSummary).ToList().AsReadOnly(),
            total, request.Page, request.PageSize);
    }
}

public record ValidateCouponQuery(string CouponCode) : IRequest<PromotionResponse>;

public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, PromotionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    public ValidateCouponQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<PromotionResponse> Handle(ValidateCouponQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var p = await _uow.Promotions.GetByCouponCodeAsync(tenantId, request.CouponCode, ct)
            ?? throw new NotFoundException("Promotion (coupon)", request.CouponCode);

        if (!p.IsApplicableNow())
            throw new Domain.Exceptions.DomainException(
                $"Coupon '{request.CouponCode}' is not currently valid.");

        return PromotionMapper.ToResponse(p);
    }
}
