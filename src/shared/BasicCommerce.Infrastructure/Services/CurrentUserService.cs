using System.Security.Claims;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace BasicCommerce.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal =>
        _httpContextAccessor.HttpContext?.User;

    public Guid UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Principal?.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

    public Guid TenantId =>
        Guid.TryParse(Principal?.FindFirstValue("tenant_id"), out var id) ? id : Guid.Empty;

    public Guid? StoreId =>
        Guid.TryParse(Principal?.FindFirstValue("store_id"), out var id) ? id : null;

    public UserRole Role =>
        Enum.TryParse<UserRole>(Principal?.FindFirstValue("role"), out var role)
            ? role : UserRole.Customer;

    public string Email =>
        Principal?.FindFirstValue(ClaimTypes.Email)
        ?? Principal?.FindFirstValue("email")
        ?? string.Empty;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true;
}
