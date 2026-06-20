export interface Customer {
  id: string;
  code: string;
  name: string;
  email: string | null;
  phone: string | null;
  addressLine1: string | null;
  city: string | null;
  loyaltyPoints: number;
  creditLimit: number;
  currentBalance: number;
  availableCredit: number;
  status: string;
  createdAt: string;
}

export interface CustomerListResponse {
  items: Customer[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface RegisterCustomerPayload {
  name: string;
  email?: string | null;
  phone?: string | null;
  creditLimit?: number;
  addressLine1?: string | null;
  city?: string | null;
  district?: string | null;
  postalCode?: string | null;
}

export interface UpdateCustomerPayload {
  name: string;
  email?: string | null;
  phone?: string | null;
  addressLine1?: string | null;
  city?: string | null;
  district?: string | null;
  postalCode?: string | null;
}

export interface CreditTransactionRow {
  id: string;
  type: string;
  amount: number;
  description: string;
  reference: string | null;
  createdAt: string;
}

export interface CreditAccount {
  id: string;
  customerId: string;
  customerName: string;
  customerCode: string;
  storeName: string;
  creditLimit: number;
  outstandingBalance: number;
  availableCredit: number;
  status: string;
  lastPaymentAt: string | null;
  recentHistory: CreditTransactionRow[];
}

export interface CustomerFormState {
  name: string;
  email: string;
  phone: string;
  creditLimit: string;
  addressLine1: string;
  city: string;
  district: string;
  postalCode: string;
}
