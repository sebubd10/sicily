using BasicCommerce.Contracts.SupplierReturns;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.SupplierReturns;

internal static class SupplierReturnMapper
{
    internal static SupplierReturnResponse ToResponse(SupplierReturn r) =>
        new(r.Id,
            r.ReturnNumber,
            r.SupplierId,
            r.Supplier?.Name ?? string.Empty,
            r.StoreId,
            r.PurchaseOrderId,
            r.ReturnStatus.ToString(),
            r.Notes,
            r.TotalReturnValue,
            r.ExpectedCreditAmount,
            r.ActualCreditAmount,
            r.CreditNoteReference,
            r.ShippedAt,
            r.CreditReceivedAt,
            r.Items.Select(ToItemResponse).ToList(),
            r.CreatedAt,
            r.UpdatedAt);

    internal static SupplierReturnSummaryResponse ToSummary(SupplierReturn r) =>
        new(r.Id,
            r.ReturnNumber,
            r.SupplierId,
            r.Supplier?.Name ?? string.Empty,
            r.StoreId,
            r.ReturnStatus.ToString(),
            r.TotalReturnValue,
            r.ExpectedCreditAmount,
            r.ActualCreditAmount,
            r.CreatedAt,
            r.ShippedAt);

    private static SupplierReturnItemResponse ToItemResponse(SupplierReturnItem i) =>
        new(i.Id,
            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Product?.Sku ?? string.Empty,
            i.Quantity,
            i.UnitCost,
            i.TotalCost,
            i.Reason.ToString(),
            i.Notes);
}
