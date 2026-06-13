using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Users;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Users.Queries;

public record GetUserQuery(Guid UserId) : IRequest<UserDetailResponse>;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDetailResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetUserQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<UserDetailResponse> Handle(GetUserQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var user = await _uow.Users.GetByIdForTenantAsync(tenantId, request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        string? storeName = null;
        if (user.StoreId.HasValue)
        {
            var store = await _uow.Stores.GetByIdAsync(user.StoreId.Value, ct);
            storeName = store?.Name;
        }

        string? userTypeName = null;
        if (user.UserTypeId.HasValue)
        {
            var userType = await _uow.UserTypes.GetByIdForTenantAsync(tenantId, user.UserTypeId.Value, ct);
            userTypeName = userType?.Name;
        }

        return new UserDetailResponse(
            user.Id,
            user.EmployeeCode,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role.ToString(),
            user.StoreId,
            storeName,
            user.PhoneNumber,
            user.Status.ToString(),
            user.LastLoginAt,
            user.UserTypeId,
            userTypeName,
            user.IsLocked);
    }
}
