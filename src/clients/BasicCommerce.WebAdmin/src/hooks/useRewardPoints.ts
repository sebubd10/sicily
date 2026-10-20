import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import * as rewardPointsApi from '../api/rewardPointsApi';
import type { RewardPointsSettings } from '../types/rewardPoints';

const SETTINGS_KEY = ['reward-points', 'settings'] as const;
const ACCOUNT_KEY  = ['reward-points', 'account'] as const;

export function useRewardPointsSettings() {
  return useQuery({
    queryKey: SETTINGS_KEY,
    queryFn: rewardPointsApi.getSettings,
    staleTime: 60_000,
  });
}

export function useUpdateRewardPointsSettings() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (settings: Omit<RewardPointsSettings, 'id'>) =>
      rewardPointsApi.updateSettings(settings),
    onSuccess: () => qc.invalidateQueries({ queryKey: SETTINGS_KEY }),
  });
}

export function useCustomerRewardPoints(customerId: string | null, storeId?: string) {
  return useQuery({
    queryKey: [...ACCOUNT_KEY, customerId, storeId],
    queryFn: () => rewardPointsApi.getCustomerPoints(customerId!, storeId),
    enabled: !!customerId,
    staleTime: 0,
  });
}

export function useManualAdjust() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: rewardPointsApi.manualAdjust,
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: [...ACCOUNT_KEY, vars.customerId] });
    },
  });
}

/** Customer's points at checkout: balance, max usable on this order, points to earn. */
export function useCheckoutRewardPoints(params: {
  customerId: string | null;
  storeId: string;
  orderTotal: number;
  pointsAmount?: number;
}) {
  const orderTotal = Math.round(params.orderTotal * 100) / 100;
  const pointsAmount = Math.round((params.pointsAmount ?? 0) * 100) / 100;
  return useQuery({
    queryKey: [...ACCOUNT_KEY, 'checkout', params.customerId, params.storeId, orderTotal, pointsAmount],
    queryFn: () => rewardPointsApi.getCheckoutPoints({
      customerId: params.customerId!,
      storeId: params.storeId,
      orderTotal,
      pointsAmount,
    }),
    enabled: !!params.customerId && !!params.storeId,
    placeholderData: keepPreviousData,
    staleTime: 10_000,
  });
}
