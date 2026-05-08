using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.UserTypes;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.UserTypes;

// ── Create ─────────────────────────────────────────────────────
public record CreateUserTypeCommand(
    string Name,
    string? Description,
    int SortOrder,
    string? Color) : IRequest<UserTypeResponse>;

public class CreateUserTypeCommandValidator : AbstractValidator<CreateUserTypeCommand>
{
    public CreateUserTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateUserTypeCommandHandler
    : IRequestHandler<CreateUserTypeCommand, UserTypeResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateUserTypeCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<UserTypeResponse> Handle(
        CreateUserTypeCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        if (await _uow.UserTypes.NameExistsAsync(tenantId, request.Name, null, ct))
            throw new DomainException($"A user type named '{request.Name}' already exists.");

        var userType = UserType.Create(tenantId, request.Name, request.Description,
            isSystem: false, request.SortOrder, request.Color);
        await _uow.UserTypes.AddAsync(userType, ct);
        await _uow.SaveChangesAsync(ct);
        return UserTypeMapper.ToResponse(userType);
    }
}

// ── Update ─────────────────────────────────────────────────────
public record UpdateUserTypeCommand(
    Guid Id, string Name, string? Description, int SortOrder, string? Color)
    : IRequest<UserTypeResponse>;

public class UpdateUserTypeCommandHandler
    : IRequestHandler<UpdateUserTypeCommand, UserTypeResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateUserTypeCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<UserTypeResponse> Handle(
        UpdateUserTypeCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var userType = await _uow.UserTypes.GetByIdForTenantAsync(tenantId, request.Id, ct)
            ?? throw new NotFoundException("UserType", request.Id);

        if (await _uow.UserTypes.NameExistsAsync(tenantId, request.Name, request.Id, ct))
            throw new DomainException($"A user type named '{request.Name}' already exists.");

        userType.Update(request.Name, request.Description, request.SortOrder, request.Color);
        _uow.UserTypes.Update(userType);
        await _uow.SaveChangesAsync(ct);
        return UserTypeMapper.ToResponse(userType);
    }
}

// ── Delete ─────────────────────────────────────────────────────
public record DeleteUserTypeCommand(Guid Id) : IRequest;

public class DeleteUserTypeCommandHandler : IRequestHandler<DeleteUserTypeCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteUserTypeCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteUserTypeCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var userType = await _uow.UserTypes.GetByIdForTenantAsync(tenantId, request.Id, ct)
            ?? throw new NotFoundException("UserType", request.Id);

        if (userType.IsSystem)
            throw new DomainException("System user types cannot be deleted.");

        userType.Status = EntityStatus.Deleted;
        _uow.UserTypes.Update(userType);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Set menu access ────────────────────────────────────────────
public record SetUserTypeMenusCommand(Guid UserTypeId, IEnumerable<Guid> SubMenuIds)
    : IRequest<UserTypeResponse>;

public class SetUserTypeMenusCommandHandler
    : IRequestHandler<SetUserTypeMenusCommand, UserTypeResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SetUserTypeMenusCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<UserTypeResponse> Handle(
        SetUserTypeMenusCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var userType = await _uow.UserTypes.GetWithAccessAsync(tenantId, request.UserTypeId, ct)
            ?? throw new NotFoundException("UserType", request.UserTypeId);

        var subMenus = await _uow.AppMenus.GetSubMenusByIdsAsync(request.SubMenuIds, ct);
        userType.SetMenuAccess(subMenus);
        _uow.UserTypes.Update(userType);
        await _uow.SaveChangesAsync(ct);
        return UserTypeMapper.ToResponseWithAccess(userType);
    }
}

// ── Set API permissions ────────────────────────────────────────
public record SetUserTypePermissionsCommand(Guid UserTypeId, IEnumerable<string> PermissionCodes)
    : IRequest<UserTypeResponse>;

public class SetUserTypePermissionsCommandHandler
    : IRequestHandler<SetUserTypePermissionsCommand, UserTypeResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permService;

    public SetUserTypePermissionsCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser,
        IPermissionService permService)
    { _uow = uow; _currentUser = currentUser; _permService = permService; }

    public async Task<UserTypeResponse> Handle(
        SetUserTypePermissionsCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var userType = await _uow.UserTypes.GetWithAccessAsync(tenantId, request.UserTypeId, ct)
            ?? throw new NotFoundException("UserType", request.UserTypeId);

        var permissions = await _uow.ApiPermissions.GetByCodesAsync(request.PermissionCodes, ct);
        userType.SetPermissions(permissions);
        _uow.UserTypes.Update(userType);
        await _uow.SaveChangesAsync(ct);

        _permService.InvalidateCache(request.UserTypeId);
        return UserTypeMapper.ToResponseWithAccess(userType);
    }
}

// ── Assign user type to a user ─────────────────────────────────
public record AssignUserTypeCommand(Guid UserId, Guid? UserTypeId) : IRequest;

public class AssignUserTypeCommandHandler : IRequestHandler<AssignUserTypeCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AssignUserTypeCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task Handle(AssignUserTypeCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var user = await _uow.Users.GetByIdForTenantAsync(tenantId, request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (request.UserTypeId.HasValue)
        {
            var userType = await _uow.UserTypes.GetByIdForTenantAsync(
                tenantId, request.UserTypeId.Value, ct)
                ?? throw new NotFoundException("UserType", request.UserTypeId.Value);
        }

        user.SetUserType(request.UserTypeId);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }
}
