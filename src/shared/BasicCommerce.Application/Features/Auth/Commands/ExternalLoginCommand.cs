using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Auth;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Auth.Commands;

public record ExternalLoginCommand(
    string Provider,
    string ExternalId,
    string Email,
    string FirstName,
    string LastName,
    string? TenantSlug) : IRequest<AuthResponse>;

public class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandValidator()
    {
        RuleFor(x => x.Provider).NotEmpty().Must(p =>
            p == "Google" || p == "Microsoft");
        RuleFor(x => x.ExternalId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, AuthResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public ExternalLoginCommandHandler(IUnitOfWork uow, IJwtService jwt)
    {
        _uow = uow;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(ExternalLoginCommand request, CancellationToken ct)
    {
        throw new NotImplementedException("Implement after Infrastructure is wired up.");
    }
}
