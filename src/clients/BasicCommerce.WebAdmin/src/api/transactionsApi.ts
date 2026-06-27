import { api } from './axiosInstance';
import type {
  Transaction,
  TransactionListResponse,
  BackofficeReturnPayload,
} from '../types/transaction';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export interface TransactionSearchParams {
  term?: string;
  storeId?: string;
  status?: string;
  type?: string;
  from?: string;
  to?: string;
  customerId?: string;
  page?: number;
  pageSize?: number;
}

export async function searchTransactions(
  params: TransactionSearchParams,
): Promise<TransactionListResponse> {
  const { data } = await api.get<ApiResponse<TransactionListResponse>>('/transactions', {
    params: {
      term: params.term || undefined,
      storeId: params.storeId || undefined,
      status: params.status || undefined,
      type: params.type || undefined,
      from: params.from || undefined,
      to: params.to || undefined,
      customerId: params.customerId || undefined,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  });
  return data.data!;
}

export async function getTransaction(id: string): Promise<Transaction> {
  const { data } = await api.get<ApiResponse<Transaction>>(`/transactions/${id}`);
  return data.data!;
}

export async function voidTransaction(id: string, reason: string): Promise<Transaction> {
  const { data } = await api.put<ApiResponse<Transaction>>(`/transactions/${id}/void`, { reason });
  return data.data!;
}

export async function createReturn(
  id: string,
  payload: BackofficeReturnPayload,
): Promise<Transaction> {
  const { data } = await api.post<ApiResponse<Transaction>>(
    `/transactions/${id}/return`,
    payload,
  );
  return data.data!;
}

export async function createTransaction(payload: {
  storeId: string;
  terminalId: string;
  customerId?: string;
}): Promise<Transaction> {
  const { data } = await api.post<ApiResponse<Transaction>>('/transactions', payload);
  return data.data!;
}

export async function addLineItem(
  txnId: string,
  productId: string,
  quantity: number,
): Promise<Transaction> {
  const { data } = await api.post<ApiResponse<Transaction>>(
    `/transactions/${txnId}/items`,
    { productId, quantity },
  );
  return data.data!;
}

export async function voidLineItem(txnId: string, lineItemId: string): Promise<Transaction> {
  const { data } = await api.delete<ApiResponse<Transaction>>(
    `/transactions/${txnId}/items/${lineItemId}`,
  );
  return data.data!;
}

export async function attachCustomer(txnId: string, customerId: string): Promise<Transaction> {
  const { data } = await api.put<ApiResponse<Transaction>>(
    `/transactions/${txnId}/customer`,
    { customerId },
  );
  return data.data!;
}

export async function addPayment(
  txnId: string,
  method: string,
  amount: number,
  reference?: string,
  giftCardCode?: string,
): Promise<Transaction> {
  const { data } = await api.post<ApiResponse<Transaction>>(
    `/transactions/${txnId}/payments`,
    { method, amount, reference: reference || undefined, giftCardCode: giftCardCode || undefined },
  );
  return data.data!;
}

export async function completeTransaction(txnId: string): Promise<Transaction> {
  const { data } = await api.post<ApiResponse<Transaction>>(
    `/transactions/${txnId}/complete`,
  );
  return data.data!;
}
