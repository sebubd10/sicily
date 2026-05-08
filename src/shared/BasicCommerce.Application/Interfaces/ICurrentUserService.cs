using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid TenantId { get; }
    Guid? StoreId { get; }
    Guid? UserTypeId { get; }
    UserRole Role { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}
