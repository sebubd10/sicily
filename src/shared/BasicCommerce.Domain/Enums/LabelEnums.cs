namespace BasicCommerce.Domain.Enums;

public enum LabelType
{
    PriceLabel = 1,
    BarcodeLabel = 2,
    ShelfLabel = 3,
    PromotionalLabel = 4,
    WeightPriceLabel = 5,
    ReceivingLabel = 6
}

public enum BarcodeSymbology
{
    EAN13 = 1,
    EAN8 = 2,
    Code128 = 3,
    QRCode = 4,
    UPCA = 5,
    ITF14 = 6,
    Code39 = 7
}

public enum LabelOutputFormat
{
    Zpl = 1,
    Html = 2,
    JsonLayout = 3
}

public enum LabelPrintJobStatus
{
    Pending = 1,
    Printing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public enum LabelFieldType
{
    ProductName = 1,
    ProductNameBn = 2,
    Price = 3,
    OriginalPrice = 4,
    CostPrice = 5,
    Barcode = 6,
    Sku = 7,
    Plu = 8,
    StoreName = 9,
    StoreAddress = 10,
    ExpiryDate = 11,
    LotNumber = 12,
    WeightUom = 13,
    PromotionalText = 14,
    VatIndicator = 15,
    CategoryName = 16,
    ManufacturerName = 17,
    CustomText = 18,
    Separator = 19,
    Logo = 20
}
