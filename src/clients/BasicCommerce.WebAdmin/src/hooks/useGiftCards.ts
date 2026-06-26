import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import * as giftCardsApi from '../api/giftCardsApi';
import type { IssueGiftCardPayload } from '../types/giftCard';

const KEY = ['gift-cards'] as const;

export function useGiftCards(params: {
  storeId?: string;
  status?: string;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: [...KEY, params],
    queryFn: () => giftCardsApi.getGiftCards(params),
    placeholderData: keepPreviousData,
    staleTime: 30_000,
  });
}

export function useGiftCardDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => giftCardsApi.getGiftCardById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useIssueGiftCard() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: IssueGiftCardPayload) => giftCardsApi.issueGiftCard(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useReloadGiftCard() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, amount, notes }: { id: string; amount: number; notes?: string | null }) =>
      giftCardsApi.reloadGiftCard(id, amount, notes),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useCancelGiftCard() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason?: string | null }) =>
      giftCardsApi.cancelGiftCard(id, reason),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
