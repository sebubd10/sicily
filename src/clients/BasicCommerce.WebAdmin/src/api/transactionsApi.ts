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
