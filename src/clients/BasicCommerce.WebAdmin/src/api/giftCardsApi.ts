import { api } from './axiosInstance';
import type { GiftCard, GiftCardListResponse, IssueGiftCardPayload } from '../types/giftCard';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getGiftCards(params: {
  storeId?: string;
  status?: string;
  page?: number;
  pageSize?: number;
}): Promise<GiftCardListResponse> {
  const { data } = await api.get<ApiResponse<GiftCardListResponse>>('/gift-cards', {
    params: {
      storeId: params.storeId || undefined,
      status: params.status || undefined,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  });
  return data.data!;
}

export async function getGiftCardById(id: string): Promise<GiftCard> {
  const { data } = await api.get<ApiResponse<GiftCard>>(`/gift-cards/${id}`);
  return data.data!;
}

export async function checkGiftCardBalance(code: string): Promise<{
  code: string; balance: number; cardStatus: string; expiryDate: string | null;
}> {
  const { data } = await api.get(`/gift-cards/check/${encodeURIComponent(code)}`);
  return data.data!;
}

export async function issueGiftCard(payload: IssueGiftCardPayload): Promise<GiftCard> {
  const { data } = await api.post<ApiResponse<GiftCard>>('/gift-cards', {
    storeId: payload.storeId,
    amount: payload.amount,
    expiryDate: payload.expiryDate ?? null,
    issuedToCustomerId: payload.issuedToCustomerId ?? null,
    notes: payload.notes ?? null,
  });
  return data.data!;
}

export async function reloadGiftCard(
  id: string,
  amount: number,
  notes?: string | null,
): Promise<GiftCard> {
  const { data } = await api.post<ApiResponse<GiftCard>>(`/gift-cards/${id}/reload`, {
    amount,
    notes: notes ?? null,
  });
  return data.data!;
}

export async function cancelGiftCard(id: string, reason?: string | null): Promise<GiftCard> {
  const { data } = await api.post<ApiResponse<GiftCard>>(`/gift-cards/${id}/cancel`, reason ?? null, {
    headers: { 'Content-Type': 'application/json' },
  });
  return data.data!;
}
