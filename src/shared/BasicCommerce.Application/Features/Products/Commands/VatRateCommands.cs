using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Commands;

public record CreateVatRateCommand(
    string Name,
    string Code,
    decimal Rate,
    bool IsDefault) : IRequest<VatRateResponse>;

public record UpdateVatRateCommand(
    Guid VatRateId,
    string Name,
    string Code,
    decimal Rate,
    bool IsDefault) : IRequest<VatRateResponse>;

public record DeactivateVatRateCommand(Guid VatRateId) : IRequest;
public record ActivateVatRateCommand(Guid VatRateId) : IRequest;
public record DeleteVatRateCommand(Guid VatRateId) : IRequest;

public class CreateVatRateCommandValidator : AbstractValidator<CreateVatRateCommand>
{
    public CreateVatRateCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Rate).InclusiveBetween(0, 100);
    }
}

public class UpdateVatRateCommandValidator : AbstractValidator<UpdateVatRateCommand>
{
    public UpdateVatRateCommandValidator()
    {
        RuleFor(x => x.VatRateId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Rate).InclusiveBetween(0, 100);
    }
}

public class CreateVatRateCommandHandler : IRequestHandler<CreateVatRateCommand, VatRateResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateVatRateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<VatRateResponse> Handle(CreateVatRateCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var code = request.Code.ToUpperInvariant();

        if (await _uow.VatRates.ExistsAsync(v => v.TenantId == tenantId && v.Code == code, ct))
            throw new DomainException($"VAT rate with code '{code}' already exists.");

        var vatRate = VatRate.Create(tenantId, request.Name, code, request.Rate, request.IsDefault);

        if (request.IsDefault)
            await ClearExistingDefaultAsync(tenantId, ct);

        await _uow.VatRates.AddAsync(vatRate, ct);
        await _uow.SaveChangesAsync(ct);
        return new VatRateResponse(vatRate.Id, vatRate.Name, vatRate.Code, vatRate.Rate, vatRate.IsDefault, vatRate.Status.ToString());
    }

    private async Task ClearExistingDefaultAsync(Guid tenantId, CancellationToken ct)
    {
        var current = await _uow.VatRates.GetDefaultAsync(tenantId, ct);
        if (current is not null)
        {
            current.Update(current.Name, current.Code, current.Rate, false);
            _uow.VatRates.Update(current);
        }
    }
}

public class UpdateVatRateCommandHandler : IRequestHandler<UpdateVatRateCommand, VatRateResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateVatRateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<VatRateResponse> Handle(UpdateVatRateCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var vatRate = await _uow.VatRates.GetByIdAsync(request.VatRateId, ct)
            ?? throw new NotFoundException("VatRate", request.VatRateId);
        if (vatRate.TenantId != tenantId) throw new NotFoundException("VatRate", request.VatRateId);

        var code = request.Code.ToUpperInvariant();
        if (await _uow.VatRates.ExistsAsync(v => v.TenantId == tenantId && v.Code == code && v.Id != vatRate.Id, ct))
            throw new DomainException($"VAT rate with code '{code}' already exists.");

        if (request.IsDefault && !vatRate.IsDefault)
            await ClearExistingDefaultAsync(tenantId, vatRate.Id, ct);

        vatRate.Update(request.Name, code, request.Rate, request.IsDefault);
        await _uow.SaveChangesAsync(ct);
        return new VatRateResponse(vatRate.Id, vatRate.Name, vatRate.Code, vatRate.Rate, vatRate.IsDefault, vatRate.Status.ToString());
    }

    private async Task ClearExistingDefaultAsync(Guid tenantId, Guid excludeId, CancellationToken ct)
    {
        var current = await _uow.VatRates.GetDefaultAsync(tenantId, ct);
        if (current is not null && current.Id != excludeId)
        {
            current.Update(current.Name, current.Code, current.Rate, false);
            _uow.VatRates.Update(current);
        }
    }
}

public class DeactivateVatRateCommandHandler : IRequestHandler<DeactivateVatRateCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateVatRateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeactivateVatRateCommand request, CancellationToken ct)
    {
        var vatRate = await _uow.VatRates.GetByIdAsync(request.VatRateId, ct)
            ?? throw new NotFoundException("VatRate", request.VatRateId);
        if (vatRate.TenantId != _currentUser.TenantId)
            throw new NotFoundException("VatRate", request.VatRateId);

        if (vatRate.IsDefault)
            throw new DomainException("Cannot deactivate the default VAT rate. Set another rate as default first.");

        vatRate.Deactivate();
        await _uow.SaveChangesAsync(ct);
    }
}

public class ActivateVatRateCommandHandler : IRequestHandler<ActivateVatRateCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ActivateVatRateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateVatRateCommand request, CancellationToken ct)
    {
        var vatRate = await _uow.VatRates.GetByIdAsync(request.VatRateId, ct)
            ?? throw new NotFoundException("VatRate", request.VatRateId);
        if (vatRate.TenantId != _currentUser.TenantId)
            throw new NotFoundException("VatRate", request.VatRateId);

        vatRate.Activate();
        await _uow.SaveChangesAsync(ct);
    }
}

public class DeleteVatRateCommandHandler : IRequestHandler<DeleteVatRateCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteVatRateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteVatRateCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var vatRate = await _uow.VatRates.GetByIdAsync(request.VatRateId, ct)
            ?? throw new NotFoundException("VatRate", request.VatRateId);
        if (vatRate.TenantId != tenantId)
            throw new NotFoundException("VatRate", request.VatRateId);

        if (vatRate.IsDefault)
            throw new DomainException("Cannot delete the default VAT rate. Set another rate as default first.");

        var productCount = await _uow.Products.CountByVatRateAsync(tenantId, vatRate.Id, ct);
        if (productCount > 0)
            throw new DomainException(
                $"Cannot delete: {productCount} product{(productCount == 1 ? " is" : "s are")} using this VAT rate. Update those products first.");

        _uow.VatRates.Remove(vatRate);
        await _uow.SaveChangesAsync(ct);
    }
}
