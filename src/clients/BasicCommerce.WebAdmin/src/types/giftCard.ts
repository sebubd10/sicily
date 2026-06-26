export type GiftCardStatus = 'Inactive' | 'Active' | 'Depleted' | 'Expired' | 'Cancelled';

export interface GiftCardTransactionItem {
  id: string;
  transactionType: 'Issue' | 'Redeem' | 'Reload' | 'Refund' | 'Expire' | 'Cancel';
  amount: number;
  balanceAfter: number;
  saleTransactionId: string | null;
  notes: string | null;
  createdAt: string;
}

export interface GiftCard {
  id: string;
  code: string;
  storeId: string;
  initialBalance: number;
  balance: number;
  cardStatus: GiftCardStatus;
  expiryDate: string | null;
  issuedToCustomerId: string | null;
  issuedInTransactionId: string | null;
  notes: string | null;
  createdAt: string;
  transactions: GiftCardTransactionItem[];
}

export interface GiftCardSummary {
  id: string;
  code: string;
  storeId: string;
  balance: number;
  cardStatus: GiftCardStatus;
  expiryDate: string | null;
  createdAt: string;
}

export interface GiftCardListResponse {
  items: GiftCardSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface IssueGiftCardPayload {
  storeId: string;
  amount: number;
  expiryDate?: string | null;
  issuedToCustomerId?: string | null;
  notes?: string | null;
}
