export type TransactionStatus = 'Open' | 'Completed' | 'Voided' | 'Suspended' | 'Refunded';
export type TransactionType = 'Sale' | 'Return' | 'Exchange' | 'CreditSale';
export type PaymentMethod = 'Cash' | 'Card' | 'BKash' | 'Nagad' | 'Rocket' | 'Ebt' | 'Credit' | 'GiftCard' | 'SslCommerz' | 'AamarPay' | 'RewardPoints' | 'ExchangeCredit';
export type PaymentStatus = 'Pending' | 'Approved' | 'Declined' | 'Refunded' | 'PendingSync';
export type ReturnReason = 'CustomerChangedMind' | 'Defective' | 'WrongItem' | 'Expired' | 'Other';
export type DamageDisposition = 'RestoreToStock' | 'WriteOff';

export interface LineItem {
  id: string;
  productName: string;
  productSku: string;
  quantity: number;
  unitPrice: number;
  taxRate: number;
  taxAmount: number;
  discountAmount: number;
  lineTotal: number;
  isVoided: boolean;
  isPriceOverridden: boolean;
  returnReason: string | null;
  damageDisposition: string | null;
  appliedPromotionId: string | null;
  appliedPromotionName: string | null;
}

export interface Payment {
  id: string;
  method: PaymentMethod;
  amount: number;
  status: PaymentStatus;
  reference: string | null;
}

export interface Transaction {
  id: string;
  transactionNumber: string;
  status: TransactionStatus;
  type: TransactionType;
  storeId: string;
  terminalId: string;
  cashierId: string;
  customerId: string | null;
  customerName: string | null;
  originalTransactionId: string | null;
  lineItems: LineItem[];
  payments: Payment[];
  subTotal: number;
  taxTotal: number;
  discountTotal: number;
  total: number;
  amountPaid: number;
  changeDue: number;
  notes: string | null;
  createdAt: string;
  completedAt: string | null;
  voidedAt: string | null;
  voidReason: string | null;
  /** Net points this sale earned (after any reversals). */
  pointsEarned: number;
  /** Net points redeemed as payment on this sale (after any refunds). */
  pointsRedeemed: number;
}

export interface TransactionSummary {
  id: string;
  transactionNumber: string;
  status: TransactionStatus;
  type: TransactionType;
  storeId: string;
  storeName: string | null;
  customerId: string | null;
  customerName: string | null;
  total: number;
  amountPaid: number;
  itemCount: number;
  createdAt: string;
  completedAt: string | null;
}

export interface TransactionListResponse {
  items: TransactionSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ReturnLineItemPayload {
  originalLineItemId: string;
  quantity: number;
  returnReason: ReturnReason;
  damageDisposition: DamageDisposition;
}

export interface BackofficeReturnPayload {
  items: ReturnLineItemPayload[];
  refundMethod: string;
  notes?: string;
}

export interface CartPreviewLine {
  productId: string;
  productName: string;
  productSku: string;
  quantity: number;
  unitPrice: number;
  lineSubTotal: number;
  discountAmount: number;
  taxRate: number;
  taxAmount: number;
  /** What the customer pays for this line: price − discount + VAT. */
  lineTotal: number;
  promotionId: string | null;
  promotionName: string | null;
}

export interface CartPreview {
  lines: CartPreviewLine[];
  subTotal: number;
  discountTotal: number;
  taxTotal: number;
  total: number;
}

export type TransactionAuditAction =
  | 'Created' | 'ItemAdded' | 'ItemQuantityChanged' | 'ItemRemoved' | 'CustomerChanged'
  | 'PaymentAdded' | 'PaymentRemoved' | 'Suspended' | 'Resumed' | 'Completed' | 'Voided'
  | 'Returned' | 'NotesChanged' | 'PaymentMethodCorrected' | 'Exchanged' | 'ReRung';

export interface TransactionAuditEntry {
  id: string;
  action: TransactionAuditAction;
  summary: string;
  reason: string | null;
  userId: string | null;
  userName: string | null;
  createdAt: string;
}

export interface CorrectTransactionPayload {
  reason: string;
  updateNotes?: boolean;
  notes?: string | null;
  attachCustomerId?: string | null;
  paymentCorrections?: { paymentId: string; newMethod: string; mobileNumber?: string | null }[];
}

export interface ExchangePayload {
  returnItems: ReturnLineItemPayload[];
  newItems: { productId: string; quantity: number }[];
  settlementMethod: string;
  settlementMobileNumber?: string | null;
  notes?: string | null;
}

export interface ExchangeResult {
  return: Transaction;
  replacement: Transaction;
  /** Positive: customer paid this extra. Negative: customer was refunded this. */
  difference: number;
}
