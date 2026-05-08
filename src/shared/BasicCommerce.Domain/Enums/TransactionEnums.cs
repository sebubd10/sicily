namespace BasicCommerce.Domain.Enums;

public enum TransactionStatus
{
    Open = 0,
    Completed = 1,
    Voided = 2,
    Suspended = 3,
    Refunded = 4
}

public enum TransactionType
{
    Sale = 0,
    Return = 1,
    Exchange = 2,
    CreditSale = 3
}

public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    BKash = 2,
    Nagad = 3,
    Rocket = 4,
    Ebt = 5,
    Credit = 6,
    GiftCard = 7,
    SslCommerz = 8,
    AamarPay = 9,
    RewardPoints = 10
}

public enum PaymentStatus
{
    Pending = 0,
    Approved = 1,
    Declined = 2,
    Refunded = 3,
    PendingSync = 4
}

public enum RewardPointsEntryType
{
    PurchaseEarned = 1,
    RegistrationEarned = 2,
    Redeemed = 3,
    Expired = 4,
    ManualAdjustment = 5
}

public enum ReturnReason
{
    CustomerChangedMind = 1,
    Defective = 2,
    WrongItem = 3,
    Expired = 4,
    Other = 5
}

public enum DamageDisposition
{
    RestoreToStock = 1,
    WriteOff = 2
}

public enum GiftCardStatus
{
    Inactive = 0,
    Active = 1,
    Depleted = 2,
    Expired = 3,
    Cancelled = 4
}

public enum GiftCardTransactionType
{
    Issue = 1,
    Redeem = 2,
    Reload = 3,
    Refund = 4,
    Expire = 5,
    Cancel = 6
}

public enum TillSessionStatus
{
    Open = 1,
    Closed = 2
}

public enum PettyTransactionType
{
    CashIn = 1,
    CashOut = 2
}

