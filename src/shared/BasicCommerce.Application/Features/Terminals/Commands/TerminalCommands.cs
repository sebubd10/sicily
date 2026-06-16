using BasicCommerce.Application.Features.Terminals;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Stores;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Terminals.Commands;

public record CreateTerminalCommand(
    Guid StoreId,
    string Name,
    string Code,
    string Type) : IRequest<TerminalResponse>;

public record UpdateTerminalCommand(
    Guid TerminalId,
    Guid StoreId,
    string Name,
    string Code,
    string Type) : IRequest<TerminalResponse>;

public record DeactivateTerminalCommand(Guid TerminalId) : IRequest;
public record ActivateTerminalCommand(Guid TerminalId) : IRequest;

public class CreateTerminalCommandValidator : AbstractValidator<CreateTerminalCommand>
{
    public CreateTerminalCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Code must be alphanumeric.");
        RuleFor(x => x.Type).Must(t => Enum.TryParse<TerminalType>(t, true, out _))
            .WithMessage("Type must be one of: Standard, SelfCheckout, MobilePOS, KioskOrder.");
    }
}

public class UpdateTerminalCommandValidator : AbstractValidator<UpdateTerminalCommand>
{
    public UpdateTerminalCommandValidator()
    {
        RuleFor(x => x.TerminalId).NotEmpty();
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Code must be alphanumeric.");
        RuleFor(x => x.Type).Must(t => Enum.TryParse<TerminalType>(t, true, out _))
            .WithMessage("Type must be one of: Standard, SelfCheckout, MobilePOS, KioskOrder.");
    }
}

public class CreateTerminalCommandHandler : IRequestHandler<CreateTerminalCommand, TerminalResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateTerminalCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TerminalResponse> Handle(CreateTerminalCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        if (await _uow.Terminals.CodeExistsAsync(tenantId, request.StoreId, request.Code, ct: ct))
            throw new DomainException($"Terminal code '{request.Code.ToUpperInvariant()}' already exists for this store.");

        var type = Enum.Parse<TerminalType>(request.Type, true);
        var terminal = Terminal.Create(tenantId, request.StoreId, request.Name, request.Code, type);

        await _uow.Terminals.AddAsync(terminal, ct);
        await _uow.SaveChangesAsync(ct);

        return TerminalMapper.ToResponse(terminal, store.Name);
    }
}

public class UpdateTerminalCommandHandler : IRequestHandler<UpdateTerminalCommand, TerminalResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateTerminalCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TerminalResponse> Handle(UpdateTerminalCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var terminal = await _uow.Terminals.GetByIdAsync(request.TerminalId, ct)
            ?? throw new NotFoundException("Terminal", request.TerminalId);
        if (terminal.TenantId != tenantId) throw new NotFoundException("Terminal", request.TerminalId);

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        if (await _uow.Terminals.CodeExistsAsync(tenantId, request.StoreId, request.Code, terminal.Id, ct))
            throw new DomainException($"Terminal code '{request.Code.ToUpperInvariant()}' already exists for this store.");

        var type = Enum.Parse<TerminalType>(request.Type, true);
        terminal.Update(request.Name, request.Code, type, request.StoreId);
        await _uow.SaveChangesAsync(ct);

        return TerminalMapper.ToResponse(terminal, store.Name);
    }
}

public class DeactivateTerminalCommandHandler : IRequestHandler<DeactivateTerminalCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateTerminalCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeactivateTerminalCommand request, CancellationToken ct)
    {
        var terminal = await _uow.Terminals.GetByIdAsync(request.TerminalId, ct)
            ?? throw new NotFoundException("Terminal", request.TerminalId);
        if (terminal.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Terminal", request.TerminalId);

        terminal.Deactivate();
        await _uow.SaveChangesAsync(ct);
    }
}

public class ActivateTerminalCommandHandler : IRequestHandler<ActivateTerminalCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ActivateTerminalCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateTerminalCommand request, CancellationToken ct)
    {
        var terminal = await _uow.Terminals.GetByIdAsync(request.TerminalId, ct)
            ?? throw new NotFoundException("Terminal", request.TerminalId);
        if (terminal.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Terminal", request.TerminalId);

        terminal.Activate();
        await _uow.SaveChangesAsync(ct);
    }
}
