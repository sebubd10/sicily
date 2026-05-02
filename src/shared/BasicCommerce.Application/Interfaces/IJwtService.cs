using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, Guid? storeId = null);
    string GenerateRefreshToken();
    bool ValidateToken(string token, out Guid userId);
}
