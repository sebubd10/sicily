namespace BasicCommerce.Domain.Enums;

public enum TerminalType
{
    Standard = 0,
    SelfCheckout = 1,
    MobilePOS = 2,
    KioskOrder = 3
}

public enum TerminalStatus
{
    Offline = 0,
    Online = 1,
    Busy = 2,
    Maintenance = 3
}
