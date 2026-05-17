using BasicCommerce.Application.Common;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Manufacturers;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Manufacturers.Commands;

public record CreateManufacturerCommand(
    string Name,
    string? Code,
    string? Country,
    string? Website,
    string? ContactEmail,
    string? Notes) : IRequest<ManufacturerResponse>;

public record UpdateManufacturerCommand(
    Guid ManufacturerId,
    string Name,
    string? Code,
    string? Country,
    string? Website,
    string? ContactEmail,
    string? Notes) : IRequest<ManufacturerResponse>;

public record DeactivateManufacturerCommand(Guid ManufacturerId) : IRequest;
public record ActivateManufacturerCommand(Guid ManufacturerId) : IRequest;

public class CreateManufacturerCommandValidator : AbstractValidator<CreateManufacturerCommand>
{
    public CreateManufacturerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).MaximumLength(50).When(x => x.Code is not null);
        RuleFor(x => x.Country)
            .Must(Countries.IsValid)
            .WithMessage("Country must be a valid ISO country code.")
            .When(x => !string.IsNullOrWhiteSpace(x.Country));
        RuleFor(x => x.Website).MaximumLength(500).When(x => x.Website is not null);
        RuleFor(x => x.ContactEmail).EmailAddress().MaximumLength(200).When(x => x.ContactEmail is not null);
    }
}

public class UpdateManufacturerCommandValidator : AbstractValidator<UpdateManufacturerCommand>
{
    public UpdateManufacturerCommandValidator()
    {
        RuleFor(x => x.ManufacturerId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).MaximumLength(50).When(x => x.Code is not null);
        RuleFor(x => x.Country)
            .Must(Countries.IsValid)
            .WithMessage("Country must be a valid ISO country code.")
            .When(x => !string.IsNullOrWhiteSpace(x.Country));
        RuleFor(x => x.Website).MaximumLength(500).When(x => x.Website is not null);
        RuleFor(x => x.ContactEmail).EmailAddress().MaximumLength(200).When(x => x.ContactEmail is not null);
    }
}

public class CreateManufacturerCommandHandler : IRequestHandler<CreateManufacturerCommand, ManufacturerResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateManufacturerCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ManufacturerResponse> Handle(CreateManufacturerCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        if (await _uow.Manufacturers.ExistsAsync(tenantId, request.Name, ct: ct))
            throw new DomainException($"Manufacturer '{request.Name}' already exists.");

        var manufacturer = Manufacturer.Create(tenantId, request.Name, request.Code,
            request.Country, request.Website, request.ContactEmail, request.Notes);
        await _uow.Manufacturers.AddAsync(manufacturer, ct);
        await _uow.SaveChangesAsync(ct);
        return ManufacturerMapper.ToResponse(manufacturer);
    }
}

public class UpdateManufacturerCommandHandler : IRequestHandler<UpdateManufacturerCommand, ManufacturerResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateManufacturerCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ManufacturerResponse> Handle(UpdateManufacturerCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var manufacturer = await _uow.Manufacturers.GetByIdAsync(request.ManufacturerId, ct)
            ?? throw new NotFoundException("Manufacturer", request.ManufacturerId);
        if (manufacturer.TenantId != tenantId) throw new NotFoundException("Manufacturer", request.ManufacturerId);

        if (await _uow.Manufacturers.ExistsAsync(tenantId, request.Name, request.ManufacturerId, ct))
            throw new DomainException($"Manufacturer '{request.Name}' already exists.");

        manufacturer.Update(request.Name, request.Code, request.Country,
            request.Website, request.ContactEmail, request.Notes);
        await _uow.SaveChangesAsync(ct);
        return ManufacturerMapper.ToResponse(manufacturer);
    }
}

public class DeactivateManufacturerCommandHandler : IRequestHandler<DeactivateManufacturerCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateManufacturerCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeactivateManufacturerCommand request, CancellationToken ct)
    {
        var manufacturer = await _uow.Manufacturers.GetByIdAsync(request.ManufacturerId, ct)
            ?? throw new NotFoundException("Manufacturer", request.ManufacturerId);
        if (manufacturer.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Manufacturer", request.ManufacturerId);
        manufacturer.Deactivate();
        await _uow.SaveChangesAsync(ct);
    }
}

public class ActivateManufacturerCommandHandler : IRequestHandler<ActivateManufacturerCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ActivateManufacturerCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateManufacturerCommand request, CancellationToken ct)
    {
        var manufacturer = await _uow.Manufacturers.GetByIdAsync(request.ManufacturerId, ct)
            ?? throw new NotFoundException("Manufacturer", request.ManufacturerId);
        if (manufacturer.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Manufacturer", request.ManufacturerId);
        manufacturer.Activate();
        await _uow.SaveChangesAsync(ct);
    }
}
