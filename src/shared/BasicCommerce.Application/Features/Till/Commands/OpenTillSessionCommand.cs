using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Till;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Till.Commands;

public record OpenTillSessionCommand(
    Guid StoreId,
    Guid TerminalId,
    decimal OpeningFloat,
    string? Notes) : IRequest<TillSessionResponse>;

public class OpenTillSessionCommandValidator : AbstractValidator<OpenTillSessionCommand>
{
    public OpenTillSessionCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.TerminalId).NotEmpty();
        RuleFor(x => x.OpeningFloat).GreaterThanOrEqualTo(0);
    }
}

public class OpenTillSessionCommandHandler
    : IRequestHandler<OpenTillSessionCommand, TillSessionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public OpenTillSessionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TillSessionResponse> Handle(
        OpenTillSessionCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var existing = await _uow.TillSessions.GetOpenSessionAsync(
            tenantId, request.TerminalId, ct);
        if (existing is not null)
            throw new DomainException(
                $"Terminal already has an open session (Id: {existing.Id}). Close it first.");

        var session = TillSession.Open(tenantId, request.StoreId, request.TerminalId,
            _currentUser.UserId, request.OpeningFloat, request.Notes);

        await _uow.TillSessions.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);
        return TillMapper.ToResponse(session);
    }
}
