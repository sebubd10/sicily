using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.ProductTags;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductTags.Queries;

public record GetProductTagQuery(Guid Id) : IRequest<ProductTagDetailResponse>;

public class GetProductTagQueryHandler
    : IRequestHandler<GetProductTagQuery, ProductTagDetailResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetProductTagQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductTagDetailResponse> Handle(
        GetProductTagQuery request, CancellationToken ct)
    {
        var tag = await _uow.ProductTags.GetByIdForTenantAsync(
            _currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("ProductTag", request.Id);

        var count = tag.Products.Count;
        return GetProductTagsQueryHandler.MapToResponse(tag, count);
    }
}
