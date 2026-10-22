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
  /** Store the card is restricted to; null = usable at any store. */
  storeId: string | null;
  initialBalance: number;
  balance: number;
  cardStatus: GiftCardStatus;
  expiryDate: string | null;
  issuedToCustomerId: string | null;
  customerName: string | null;
  issuedInTransactionId: string | null;
  notes: string | null;
  createdAt: string;
  transactions: GiftCardTransactionItem[];
}

export interface GiftCardSummary {
  id: string;
  code: string;
  /** null = usable at any store. */
  storeId: string | null;
  issuedToCustomerId: string | null;
  customerName: string | null;
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
  /** Omit/null to make the card usable at every store. */
  storeId?: string | null;
  amount: number;
  expiryDate?: string | null;
  issuedToCustomerId: string;
  notes?: string | null;
}
