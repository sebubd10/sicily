using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Suppliers;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Suppliers.Commands;

public record CreateSupplierCommand(
    string Name,
    string Code,
    string? ContactName,
    string? Email,
    string? Phone,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? District,
    string? PostalCode,
    string? Country,
    int LeadTimeDays,
    string? Notes,
    Guid? ManufacturerId) : IRequest<SupplierResponse>;

public record UpdateSupplierCommand(
    Guid SupplierId,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? District,
    string? PostalCode,
    string? Country,
    int LeadTimeDays,
    string? Notes,
    Guid? ManufacturerId) : IRequest<SupplierResponse>;

public record DeactivateSupplierCommand(Guid SupplierId) : IRequest;
public record ActivateSupplierCommand(Guid SupplierId) : IRequest;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(200).When(x => x.Email is not null);
        RuleFor(x => x.LeadTimeDays).GreaterThanOrEqualTo(0);
    }
}

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(200).When(x => x.Email is not null);
        RuleFor(x => x.LeadTimeDays).GreaterThanOrEqualTo(0);
    }
}

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, SupplierResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateSupplierCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierResponse> Handle(CreateSupplierCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        if (await _uow.Suppliers.ExistsAsync(tenantId, request.Code.ToUpperInvariant(), ct: ct))
            throw new DomainException($"Supplier code '{request.Code}' already exists.");

        Address? address = null;
        if (request.AddressLine1 is not null && request.City is not null)
            address = Address.Create(request.AddressLine1, request.City,
                request.District ?? string.Empty, request.PostalCode ?? string.Empty,
                request.AddressLine2, request.Country ?? "BD");

        var supplier = Supplier.Create(tenantId, request.Name, request.Code,
            request.ContactName, request.Email, request.Phone, address,
            request.LeadTimeDays, request.Notes, request.ManufacturerId);
        await _uow.Suppliers.AddAsync(supplier, ct);
        await _uow.SaveChangesAsync(ct);
        return SupplierMapper.ToResponse(supplier);
    }
}

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, SupplierResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateSupplierCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierResponse> Handle(UpdateSupplierCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId, ct)
            ?? throw new NotFoundException("Supplier", request.SupplierId);
        if (supplier.TenantId != tenantId) throw new NotFoundException("Supplier", request.SupplierId);

        Address? address = null;
        if (request.AddressLine1 is not null && request.City is not null)
            address = Address.Create(request.AddressLine1, request.City,
                request.District ?? string.Empty, request.PostalCode ?? string.Empty,
                request.AddressLine2, request.Country ?? "BD");

        supplier.Update(request.Name, request.ContactName, request.Email, request.Phone,
            address, request.LeadTimeDays, request.Notes, request.ManufacturerId);
        await _uow.SaveChangesAsync(ct);
        return SupplierMapper.ToResponse(supplier);
    }
}

public class DeactivateSupplierCommandHandler : IRequestHandler<DeactivateSupplierCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateSupplierCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeactivateSupplierCommand request, CancellationToken ct)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId, ct)
            ?? throw new NotFoundException("Supplier", request.SupplierId);
        if (supplier.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Supplier", request.SupplierId);
        supplier.Deactivate();
        await _uow.SaveChangesAsync(ct);
    }
}

public class ActivateSupplierCommandHandler : IRequestHandler<ActivateSupplierCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ActivateSupplierCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateSupplierCommand request, CancellationToken ct)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId, ct)
            ?? throw new NotFoundException("Supplier", request.SupplierId);
        if (supplier.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Supplier", request.SupplierId);
        supplier.Activate();
        await _uow.SaveChangesAsync(ct);
    }
}
