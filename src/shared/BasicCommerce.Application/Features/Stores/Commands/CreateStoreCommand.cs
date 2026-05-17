using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Stores;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Stores.Commands;

public record CreateStoreCommand(
    string Name,
    string Code,
    string AddressLine1,
    string City,
    string District,
    string PostalCode,
    string? Phone,
    string? Email) : IRequest<StoreResponse>;

public class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Code must be alphanumeric.");
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(300);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, StoreResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateStoreCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StoreResponse> Handle(CreateStoreCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        if (await _uow.Stores.CodeExistsAsync(tenantId, request.Code, ct))
            throw new DomainException($"Store code '{request.Code.ToUpperInvariant()}' already exists.");

        var address = new Address(
            request.AddressLine1,
            null,
            request.City,
            request.District,
            request.PostalCode);

        var store = Store.Create(tenantId, request.Name, request.Code, address,
            request.Phone, request.Email);

        await _uow.Stores.AddAsync(store, ct);
        await _uow.SaveChangesAsync(ct);

        return new StoreResponse(
            store.Id,
            store.Name,
            store.Code,
            $"{store.Address.Line1}, {store.Address.City}",
            store.Phone,
            store.Email,
            store.Status.ToString(),
            0);
    }
}
