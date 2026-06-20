export interface PettyTransaction {
  id: string;
  type: string;
  amount: number;
  reason: string;
  performedBy: string;
  createdAt: string;
}

export interface TillSessionSummary {
  id: string;
  storeId: string;
  storeName: string | null;
  terminalId: string;
  terminalName: string | null;
  openedBy: string;
  openedByName: string | null;
  closedBy: string | null;
  closedByName: string | null;
  openingFloat: number;
  sessionStatus: string;
  openedAt: string;
  closedAt: string | null;
}

export interface TillSession {
  id: string;
  storeId: string;
  terminalId: string;
  openedBy: string;
  closedBy: string | null;
  openingFloat: number;
  closingBalance: number | null;
  closingVariance: number | null;
  expectedClosingBalance: number | null;
  sessionStatus: string;
  openedAt: string;
  closedAt: string | null;
  notes: string | null;
  pettyTransactions: PettyTransaction[];
}

export interface TillSessionListResponse {
  items: TillSessionSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface TillReport {
  sessionId: string;
  openingFloat: number;
  cashSalesTotal: number;
  cardSalesTotal: number;
  giftCardSalesTotal: number;
  totalRefunds: number;
  pettyCashIn: number;
  pettyCashOut: number;
  expectedCash: number;
  actualCash: number | null;
  variance: number | null;
  isFinal: boolean;
}
