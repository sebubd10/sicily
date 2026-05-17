namespace BasicCommerce.Contracts.Reports;

public record DailySalesReportResponse(
    DateOnly Date,
    Guid? StoreId,
    string? StoreName,
    int CompletedTransactions,
    int VoidedTransactions,
    decimal GrossRevenue,
    decimal TaxTotal,
    decimal DiscountTotal,
    decimal NetRevenue,
    IEnumerable<PaymentMethodSummary> PaymentBreakdown);

public record PaymentMethodSummary(
    string Method,
    int TransactionCount,
    decimal TotalAmount);

public record CashierPerformanceReportResponse(
    DateTime From,
    DateTime To,
    IEnumerable<CashierPerformanceSummary> Cashiers);

public record CashierPerformanceSummary(
    Guid CashierId,
    string CashierName,
    int CompletedTransactions,
    int VoidedTransactions,
    int PriceOverrides,
    decimal TotalRevenue,
    decimal AverageTransactionValue);

public record ReconciliationReportResponse(
    DateOnly Date,
    Guid TerminalId,
    string TerminalName,
    string TerminalCode,
    decimal OpeningFloat,
    decimal CashSales,
    decimal ExpectedCashInDrawer,
    decimal TotalSales,
    int TransactionCount,
    IEnumerable<PaymentMethodSummary> PaymentBreakdown);

public record StockReportResponse(
    Guid StoreId,
    string StoreName,
    int TotalProducts,
    int LowStockCount,
    int OutOfStockCount,
    IEnumerable<StockItemRow> Items);

public record StockItemRow(
    Guid ProductId,
    string ProductName,
    string Sku,
    string Barcode,
    string CategoryName,
    decimal Quantity,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal LowStockThreshold,
    bool IsLowStock,
    bool IsOutOfStock);
