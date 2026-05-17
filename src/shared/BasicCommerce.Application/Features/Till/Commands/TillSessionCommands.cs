using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Till;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Till.Commands;

// Petty cash in
public record PettyCashInCommand(Guid SessionId, decimal Amount, string Reason)
    : IRequest<TillSessionResponse>;

// Petty cash out
public record PettyCashOutCommand(Guid SessionId, decimal Amount, string Reason)
    : IRequest<TillSessionResponse>;

public class PettyCashCommandValidator<T> : AbstractValidator<T>
    where T : PettyCashInCommand
{
    public PettyCashCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(200);
    }
}

public class PettyCashInCommandHandler : IRequestHandler<PettyCashInCommand, TillSessionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public PettyCashInCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TillSessionResponse> Handle(PettyCashInCommand request, CancellationToken ct)
    {
        var session = await _uow.TillSessions.GetWithPettyAsync(
            _currentUser.TenantId, request.SessionId, ct)
            ?? throw new NotFoundException("TillSession", request.SessionId);

        session.AddPettyCash(PettyTransactionType.CashIn,
            request.Amount, request.Reason, _currentUser.UserId);
        _uow.TillSessions.Update(session);
        await _uow.SaveChangesAsync(ct);
        return TillMapper.ToResponse(session);
    }
}

public class PettyCashOutCommandHandler : IRequestHandler<PettyCashOutCommand, TillSessionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public PettyCashOutCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TillSessionResponse> Handle(PettyCashOutCommand request, CancellationToken ct)
    {
        var session = await _uow.TillSessions.GetWithPettyAsync(
            _currentUser.TenantId, request.SessionId, ct)
            ?? throw new NotFoundException("TillSession", request.SessionId);

        session.AddPettyCash(PettyTransactionType.CashOut,
            request.Amount, request.Reason, _currentUser.UserId);
        _uow.TillSessions.Update(session);
        await _uow.SaveChangesAsync(ct);
        return TillMapper.ToResponse(session);
    }
}
