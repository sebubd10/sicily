using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Queries;

public record GetVatRatesQuery : IRequest<IEnumerable<VatRateResponse>>;

public record GetVatRateQuery(Guid VatRateId) : IRequest<VatRateResponse>;

public class GetVatRatesQueryHandler : IRequestHandler<GetVatRatesQuery, IEnumerable<VatRateResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetVatRatesQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<VatRateResponse>> Handle(GetVatRatesQuery request, CancellationToken ct)
    {
        var all = await _uow.VatRates.GetAllForTenantAsync(_currentUser.TenantId, ct);
        return all.Select(v => new VatRateResponse(v.Id, v.Name, v.Code, v.Rate, v.IsDefault, v.Status.ToString()));
    }
}

public class GetVatRateQueryHandler : IRequestHandler<GetVatRateQuery, VatRateResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetVatRateQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<VatRateResponse> Handle(GetVatRateQuery request, CancellationToken ct)
    {
        var vr = await _uow.VatRates.GetByIdAsync(request.VatRateId, ct)
            ?? throw new NotFoundException("VatRate", request.VatRateId);

        if (vr.TenantId != _currentUser.TenantId)
            throw new NotFoundException("VatRate", request.VatRateId);

        return new VatRateResponse(vr.Id, vr.Name, vr.Code, vr.Rate, vr.IsDefault, vr.Status.ToString());
    }
}
