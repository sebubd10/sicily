export const TERMINAL_TYPES = ['Standard', 'SelfCheckout', 'MobilePOS', 'KioskOrder'] as const;
export type TerminalType = (typeof TERMINAL_TYPES)[number];

export interface Terminal {
  id: string;
  storeId: string;
  storeName: string;
  name: string;
  code: string;
  type: string;
  status: string;
  operationalStatus: string;
  currentCashierName: string | null;
}

export interface TerminalFormData {
  storeId: string;
  name: string;
  code: string;
  type: TerminalType;
}
