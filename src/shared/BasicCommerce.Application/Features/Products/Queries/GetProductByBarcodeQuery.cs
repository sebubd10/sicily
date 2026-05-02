using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Queries;

public record GetProductByBarcodeQuery(Guid TenantId, string Barcode) : IRequest<ProductResponse>;

public class GetProductByBarcodeQueryValidator : AbstractValidator<GetProductByBarcodeQuery>
{
    public GetProductByBarcodeQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Barcode).NotEmpty().MaximumLength(50);
    }
}

public class GetProductByBarcodeQueryHandler
    : IRequestHandler<GetProductByBarcodeQuery, ProductResponse>
{
    private readonly IUnitOfWork _uow;

    public GetProductByBarcodeQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProductResponse> Handle(GetProductByBarcodeQuery request, CancellationToken ct)
    {
        throw new NotImplementedException("Implement after Infrastructure is wired up.");
    }
}
