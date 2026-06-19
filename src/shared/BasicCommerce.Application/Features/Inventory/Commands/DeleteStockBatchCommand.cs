using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Commands;

public record DeleteStockBatchCommand(Guid StockBatchId) : IRequest;

public class DeleteStockBatchCommandHandler : IRequestHandler<DeleteStockBatchCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteStockBatchCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteStockBatchCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var batch = await _uow.StockBatches.GetByIdAsync(request.StockBatchId, ct)
            ?? throw new NotFoundException("StockBatch", request.StockBatchId);
        if (batch.TenantId != tenantId)
            throw new NotFoundException("StockBatch", request.StockBatchId);

        var reasons = new List<string>();

        if (batch.RemainingQuantity > 0)
            reasons.Add($"the batch still has {batch.RemainingQuantity:0.##} unit(s) remaining — write off or consume the stock first");

        if (reasons.Count > 0)
            throw new DomainException(
                $"Cannot delete batch '{batch.LotNumber ?? batch.Id.ToString()[..8].ToUpperInvariant()}': {string.Join("; ", reasons)}.");

        batch.SoftDelete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
