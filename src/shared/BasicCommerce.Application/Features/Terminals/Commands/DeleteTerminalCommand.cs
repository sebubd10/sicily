using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Terminals.Commands;

public record DeleteTerminalCommand(Guid TerminalId) : IRequest;

public class DeleteTerminalCommandHandler : IRequestHandler<DeleteTerminalCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteTerminalCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteTerminalCommand request, CancellationToken ct)
    {
        var terminal = await _uow.Terminals.GetByIdAsync(request.TerminalId, ct)
            ?? throw new NotFoundException("Terminal", request.TerminalId);
        if (terminal.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Terminal", request.TerminalId);

        terminal.SoftDelete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
