using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Users.Commands;

public record DeleteUserCommand(Guid UserId) : IRequest;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteUserCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var user = await _uow.Users.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (user.TenantId != tenantId)
            throw new NotFoundException("User", request.UserId);

        if (user.Id == _currentUser.UserId)
            throw new DomainException("Cannot delete your own account.");

        var reasons = new List<string>();

        if (await _uow.TillSessions.HasOpenSessionForUserAsync(tenantId, user.Id, ct))
            reasons.Add("the user has an open till session — close the session first");

        if (reasons.Count > 0)
            throw new DomainException(
                $"Cannot delete user '{user.FullName}': {string.Join("; ", reasons)}.");

        user.SoftDelete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
