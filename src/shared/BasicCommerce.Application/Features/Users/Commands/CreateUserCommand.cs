using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Users;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Users.Commands;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    Guid? StoreId,
    string? PhoneNumber) : IRequest<UserListResponse>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => Enum.TryParse<UserRole>(r, true, out _))
            .WithMessage("Invalid role. Valid roles: Cashier, Supervisor, StoreManager, ChainAdmin, SystemAdmin.");
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordHasher _hasher;

    public CreateUserCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, IPasswordHasher hasher)
    {
        _uow = uow;
        _currentUser = currentUser;
        _hasher = hasher;
    }

    public async Task<UserListResponse> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        if (await _uow.Users.GetByEmailAcrossTenantsAsync(request.Email, ct) is not null)
            throw new DomainException($"Email '{request.Email}' is already registered.");

        var role = Enum.Parse<UserRole>(request.Role, true);

        if (request.StoreId.HasValue)
        {
            var store = await _uow.Stores.GetByIdAsync(request.StoreId.Value, ct)
                ?? throw new NotFoundException("Store", request.StoreId.Value);
            if (store.TenantId != tenantId)
                throw new NotFoundException("Store", request.StoreId.Value);
        }

        var employeeCode = await GenerateEmployeeCodeAsync(tenantId, ct);
        var passwordHash = _hasher.Hash(request.Password);

        var user = User.Create(tenantId, employeeCode, request.FirstName, request.LastName,
            request.Email, passwordHash, role, request.StoreId);

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return new UserListResponse(
            user.Id,
            user.EmployeeCode,
            user.FullName,
            user.Email,
            user.Role.ToString(),
            null,
            user.Status.ToString(),
            user.LastLoginAt);
    }

    private async Task<string> GenerateEmployeeCodeAsync(Guid tenantId, CancellationToken ct)
    {
        var prefix = tenantId.ToString("N")[..4].ToUpper();
        for (var i = 0; i < 10; i++)
        {
            var code = $"{prefix}{Random.Shared.Next(1000, 9999)}";
            if (await _uow.Users.GetByEmployeeCodeAsync(tenantId, code, ct) is null)
                return code;
        }
        return $"{prefix}{Guid.NewGuid():N}"[..12];
    }
}
