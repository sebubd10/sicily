using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Users;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Users.Commands;

public record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    Guid? StoreId,
    string? PhoneNumber) : IRequest<UserListResponse>;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => Enum.TryParse<UserRole>(r, true, out _))
            .WithMessage("Invalid role. Valid roles: Cashier, Supervisor, StoreManager, ChainAdmin, SystemAdmin.");
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateUserCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<UserListResponse> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var user = await _uow.Users.GetByIdForTenantAsync(tenantId, request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        var emailConflict = await _uow.Users.GetByEmailAcrossTenantsAsync(request.Email, ct);
        if (emailConflict is not null && emailConflict.Id != request.UserId)
            throw new DomainException($"Email '{request.Email}' is already registered.");

        if (request.StoreId.HasValue)
        {
            var store = await _uow.Stores.GetByIdAsync(request.StoreId.Value, ct)
                ?? throw new NotFoundException("Store", request.StoreId.Value);
            if (store.TenantId != tenantId)
                throw new NotFoundException("Store", request.StoreId.Value);
        }

        var role = Enum.Parse<UserRole>(request.Role, true);
        user.UpdateProfile(request.FirstName, request.LastName, request.Email,
            request.PhoneNumber, role, request.StoreId);

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

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

        return new UserListResponse(
            user.Id,
            user.EmployeeCode,
            user.FullName,
            user.Email,
            user.Role.ToString(),
            storeName,
            user.Status.ToString(),
            user.LastLoginAt,
            user.UserTypeId,
            userTypeName,
            user.IsLocked);
    }
}
