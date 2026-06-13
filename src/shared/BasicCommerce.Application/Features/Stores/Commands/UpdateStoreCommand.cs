using BasicCommerce.Application.Features.Stores;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Stores;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Stores.Commands;

public record UpdateStoreCommand(
    Guid StoreId,
    string Name,
    string AddressLine1,
    string City,
    string District,
    string PostalCode,
    string? AddressLine2,
    string? Phone,
    string? Email,
    string OpeningTime,
    string ClosingTime) : IRequest<StoreResponse>;

public class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
{
    public UpdateStoreCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(300);
        RuleFor(x => x.AddressLine2).MaximumLength(300).When(x => x.AddressLine2 is not null);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.OpeningTime).Matches("^([01]\\d|2[0-3]):[0-5]\\d$").WithMessage("Opening time must be in HH:mm format.");
        RuleFor(x => x.ClosingTime).Matches("^([01]\\d|2[0-3]):[0-5]\\d$").WithMessage("Closing time must be in HH:mm format.");
    }
}

public class UpdateStoreCommandHandler : IRequestHandler<UpdateStoreCommand, StoreResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateStoreCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StoreResponse> Handle(UpdateStoreCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        var address = new Address(
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.District,
            request.PostalCode);

        store.Update(
            request.Name,
            address,
            request.Phone,
            request.Email,
            TimeOnly.Parse(request.OpeningTime),
            TimeOnly.Parse(request.ClosingTime));

        await _uow.SaveChangesAsync(ct);

        var terminals = await _uow.Terminals.GetByStoreAsync(tenantId, store.Id, ct);
        return StoreMapper.ToResponse(store, terminals.Count());
    }
}
