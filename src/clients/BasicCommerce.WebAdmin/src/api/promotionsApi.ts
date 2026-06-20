import { api } from './axiosInstance';
import type { Promotion, PromotionListResponse } from '../types/promotion';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getPromotions(params: {
  status?: string;
  page?: number;
  pageSize?: number;
}): Promise<PromotionListResponse> {
  const { data } = await api.get<ApiResponse<PromotionListResponse>>('/promotions', {
    params: {
      status: params.status || undefined,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  });
  return data.data!;
}

export async function getPromotion(id: string): Promise<Promotion> {
  const { data } = await api.get<ApiResponse<Promotion>>(`/promotions/${id}`);
  return data.data!;
}

export async function createPromotion(payload: {
  name: string;
  description?: string | null;
  type: string;
  productId?: string | null;
  categoryId?: string | null;
  storeId?: string | null;
  discountPercentage?: number | null;
  discountAmount?: number | null;
  buyQuantity?: number | null;
  getQuantity?: number | null;
  minimumCartValue?: number | null;
  couponCode?: string | null;
  requiresCoupon: boolean;
  startsAt?: string | null;
  endsAt?: string | null;
  maxUses?: number | null;
}): Promise<Promotion> {
  const { data } = await api.post<ApiResponse<Promotion>>('/promotions', payload);
  return data.data!;
}

export async function activatePromotion(id: string): Promise<Promotion> {
  const { data } = await api.post<ApiResponse<Promotion>>(`/promotions/${id}/activate`);
  return data.data!;
}

export async function pausePromotion(id: string): Promise<Promotion> {
  const { data } = await api.post<ApiResponse<Promotion>>(`/promotions/${id}/pause`);
  return data.data!;
}

export async function cancelPromotion(id: string): Promise<Promotion> {
  const { data } = await api.post<ApiResponse<Promotion>>(`/promotions/${id}/cancel`);
  return data.data!;
}
