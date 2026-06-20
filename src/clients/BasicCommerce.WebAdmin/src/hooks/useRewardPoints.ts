import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
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
