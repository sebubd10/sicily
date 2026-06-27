export type TransactionStatus = 'Open' | 'Completed' | 'Voided' | 'Suspended' | 'Refunded';
export type TransactionType = 'Sale' | 'Return' | 'Exchange' | 'CreditSale';
export type PaymentMethod = 'Cash' | 'Card' | 'BKash' | 'Nagad' | 'Rocket' | 'Ebt' | 'Credit' | 'GiftCard' | 'SslCommerz' | 'AamarPay' | 'RewardPoints';
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
