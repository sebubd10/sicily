using BasicCommerce.Contracts.SupplierReturns;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Application.Features.SupplierReturns;

internal static class SupplierReturnMapper
{
    internal static SupplierReturnResponse ToResponse(SupplierReturn r) =>
        new(r.Id,
            r.ReturnNumber,
            r.SupplierId,
            r.Supplier?.Name ?? string.Empty,
            r.StoreId,
            r.Store?.Name ?? string.Empty,
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
            r.Store?.Name ?? string.Empty,
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
            i.Product?.Status == EntityStatus.Active,
            i.Quantity,
            i.UnitCost,
            i.TotalCost,
            i.Reason.ToString(),
            i.Notes);
}
