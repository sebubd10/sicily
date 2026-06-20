import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as promotionsApi from '../api/promotionsApi';

const KEY = ['promotions'] as const;

export function usePromotions(params: { status?: string; page?: number; pageSize?: number }) {
  return useQuery({
    queryKey: [...KEY, params],
    queryFn: () => promotionsApi.getPromotions(params),
    staleTime: 30_000,
  });
}

export function usePromotion(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => promotionsApi.getPromotion(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useCreatePromotion() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: promotionsApi.createPromotion,
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useActivatePromotion() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => promotionsApi.activatePromotion(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function usePausePromotion() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => promotionsApi.pausePromotion(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useCancelPromotion() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => promotionsApi.cancelPromotion(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
