import { api } from './axiosInstance';
import type {
  Customer,
  CustomerListResponse,
  RegisterCustomerPayload,
  UpdateCustomerPayload,
  CreditAccount,
  CreditAccountListResponse,
} from '../types/customer';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function searchCustomers(
  term: string,
  page = 1,
  pageSize = 20,
): Promise<CustomerListResponse> {
  const { data } = await api.get<ApiResponse<CustomerListResponse>>('/customers', {
    params: { term: term || undefined, page, pageSize },
  });
  return data.data!;
}

export async function getCustomerById(id: string): Promise<Customer> {
  const { data } = await api.get<ApiResponse<Customer>>(`/customers/${id}`);
  return data.data!;
}

export async function registerCustomer(payload: RegisterCustomerPayload): Promise<Customer> {
  const { data } = await api.post<ApiResponse<Customer>>('/customers', payload);
  return data.data!;
}

export async function updateCustomer(
  id: string,
  payload: UpdateCustomerPayload,
): Promise<Customer> {
  const { data } = await api.put<ApiResponse<Customer>>(`/customers/${id}`, payload);
  return data.data!;
}

export async function deactivateCustomer(id: string): Promise<void> {
  await api.put(`/customers/${id}/deactivate`);
}

export async function updateCreditLimit(id: string, creditLimit: number): Promise<Customer> {
  const { data } = await api.put<ApiResponse<Customer>>(`/customers/${id}/credit-limit`, {
    creditLimit,
  });
  return data.data!;
}

export async function getAllCreditAccounts(params: {
  term?: string;
  hasBalance?: boolean;
  page?: number;
  pageSize?: number;
}): Promise<CreditAccountListResponse> {
  const { data } = await api.get<ApiResponse<CreditAccountListResponse>>(
    '/customers/credit-accounts',
    {
      params: {
        term: params.term || undefined,
        hasBalance: params.hasBalance,
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 20,
      },
    },
  );
  return data.data!;
}

export async function getCustomerCreditAccounts(customerId: string): Promise<CreditAccount[]> {
  const { data } = await api.get<ApiResponse<CreditAccount[]>>(
    `/customers/${customerId}/credit-accounts`,
  );
  return data.data!;
}

export async function getCreditAccount(creditAccountId: string): Promise<CreditAccount> {
  const { data } = await api.get<ApiResponse<CreditAccount>>(
    `/customers/credit-accounts/${creditAccountId}`,
  );
  return data.data!;
}

export async function recordCreditPayment(
  creditAccountId: string,
  amount: number,
  reference?: string | null,
): Promise<CreditAccount> {
  const { data } = await api.post<ApiResponse<CreditAccount>>(
    `/customers/credit-accounts/${creditAccountId}/payments`,
    { amount, reference: reference || null },
  );
  return data.data!;
}

export async function addLoyaltyPoints(
  id: string,
  points: number,
  reason: string,
): Promise<Customer> {
  const { data } = await api.post<ApiResponse<Customer>>(`/customers/${id}/loyalty/add`, {
    points,
    reason,
  });
  return data.data!;
}

export async function redeemLoyaltyPoints(
  id: string,
  points: number,
  reason: string,
): Promise<Customer> {
  const { data } = await api.post<ApiResponse<Customer>>(`/customers/${id}/loyalty/redeem`, {
    points,
    reason,
  });
  return data.data!;
}
