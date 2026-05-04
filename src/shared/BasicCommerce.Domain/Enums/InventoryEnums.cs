namespace BasicCommerce.Domain.Enums;

public enum StockMovementType
{
    Receive = 1,
    Adjustment = 2,
    WriteOff = 3,
    TransferOut = 4,
    TransferIn = 5,
    Sale = 6,
    Return = 7
}

public enum WarehouseMovementType
{
    PurchaseOrderReceipt = 1,
    TransferToStore = 2,
    Adjustment = 3,
    WriteOff = 4,
    Return = 5
}
