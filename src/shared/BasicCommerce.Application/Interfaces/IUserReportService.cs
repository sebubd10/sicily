using BasicCommerce.Contracts.Users;

namespace BasicCommerce.Application.Interfaces;

public interface IUserReportService
{
    byte[] Generate(
        IEnumerable<UserReportRow> users,
        string? search = null);
}
