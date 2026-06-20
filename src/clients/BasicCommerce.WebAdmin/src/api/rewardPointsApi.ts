import { api } from './axiosInstance';
import type { RewardPointsSettings, RewardPointsAccount } from '../types/rewardPoints';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getSettings(): Promise<RewardPointsSettings> {
  const { data } = await api.get<ApiResponse<RewardPointsSettings>>('/reward-points/settings');
  return data.data!;
}

export async function updateSettings(settings: Omit<RewardPointsSettings, 'id'>): Promise<RewardPointsSettings> {
  const { data } = await api.put<ApiResponse<RewardPointsSettings>>('/reward-points/settings', settings);
  return data.data!;
}

export async function getCustomerPoints(
  customerId: string,
  storeId?: string,
): Promise<RewardPointsAccount> {
  const { data } = await api.get<ApiResponse<RewardPointsAccount>>(
    `/reward-points/customers/${customerId}`,
    { params: { storeId: storeId || undefined } },
  );
  return data.data!;
}

export async function manualAdjust(payload: {
  customerId: string;
  storeId?: string;
  points: number;
  notes: string;
}): Promise<RewardPointsAccount> {
  const { data } = await api.post<ApiResponse<RewardPointsAccount>>('/reward-points/adjust', {
    customerId: payload.customerId,
    storeId: payload.storeId ?? null,
    points: payload.points,
    notes: payload.notes,
  });
  return data.data!;
}
