using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Manufacturers;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Manufacturers.Queries;

public record GetManufacturersQuery : IRequest<IEnumerable<ManufacturerResponse>>;
public record GetManufacturerQuery(Guid ManufacturerId) : IRequest<ManufacturerResponse>;

public class GetManufacturersQueryHandler : IRequestHandler<GetManufacturersQuery, IEnumerable<ManufacturerResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetManufacturersQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<ManufacturerResponse>> Handle(GetManufacturersQuery request, CancellationToken ct)
    {
        var all = await _uow.Manufacturers.GetAllForTenantAsync(_currentUser.TenantId, ct);
        return all.Select(ManufacturerMapper.ToResponse);
    }
}

public class GetManufacturerQueryHandler : IRequestHandler<GetManufacturerQuery, ManufacturerResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetManufacturerQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ManufacturerResponse> Handle(GetManufacturerQuery request, CancellationToken ct)
    {
        var manufacturer = await _uow.Manufacturers.GetByIdAsync(request.ManufacturerId, ct)
            ?? throw new NotFoundException("Manufacturer", request.ManufacturerId);
        if (manufacturer.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Manufacturer", request.ManufacturerId);
        return ManufacturerMapper.ToResponse(manufacturer);
    }
}
