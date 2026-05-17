namespace BasicCommerce.Domain.Enums;

public enum StockMovementType
{
    Receive = 1,
    Adjustment = 2,
    WriteOff = 3,
    TransferOut = 4,
    TransferIn = 5,
    Sale = 6,
    Return = 7,
    ExpiryWriteOff = 8,
    SupplierReturn = 9
}

public enum SupplierReturnStatus
{
    Draft = 1,
    Submitted = 2,
    Shipped = 3,
    CreditReceived = 4,
    Cancelled = 5
}

public enum SupplierReturnReason
{
    Damaged = 1,
    Defective = 2,
    WrongItemShipped = 3,
    Overstock = 4,
    QualityIssue = 5,
    Other = 6
}

public enum WarehouseMovementType
{
    PurchaseOrderReceipt = 1,
    TransferToStore = 2,
    Adjustment = 3,
    WriteOff = 4,
    Return = 5
}
