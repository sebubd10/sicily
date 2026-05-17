using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Customers;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Commands;

public record UpdateCustomerCommand(
    Guid CustomerId,
    string Name,
    string? Email,
    string? Phone,
    string? AddressLine1,
    string? City,
    string? District,
    string? PostalCode) : IRequest<CustomerResponse>;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateCustomerCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CustomerResponse> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        if (customer.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Customer", request.CustomerId);

        if (!string.IsNullOrWhiteSpace(request.Phone) && request.Phone != customer.Phone)
        {
            var existing = await _uow.Customers.GetByPhoneAsync(_currentUser.TenantId, request.Phone, ct);
            if (existing is not null && existing.Id != customer.Id)
                throw new DomainException($"Phone '{request.Phone}' is already registered.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != customer.Email)
        {
            var existing = await _uow.Customers.GetByEmailAsync(_currentUser.TenantId, request.Email, ct);
            if (existing is not null && existing.Id != customer.Id)
                throw new DomainException($"Email '{request.Email}' is already registered.");
        }

        Address? address = null;
        if (!string.IsNullOrWhiteSpace(request.AddressLine1) &&
            !string.IsNullOrWhiteSpace(request.City))
        {
            address = new Address(
                request.AddressLine1, null,
                request.City,
                request.District ?? string.Empty,
                request.PostalCode ?? string.Empty);
        }

        customer.Update(request.Name, request.Email, request.Phone, address);
        await _uow.SaveChangesAsync(ct);
        return RegisterCustomerCommandHandler.ToResponse(customer);
    }
}
