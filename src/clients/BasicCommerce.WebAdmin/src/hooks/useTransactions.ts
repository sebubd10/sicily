import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import * as transactionsApi from '../api/transactionsApi';
import type { BackofficeReturnPayload } from '../types/transaction';

const KEY = ['transactions'] as const;

export function useTransactions(params: transactionsApi.TransactionSearchParams) {
  return useQuery({
    queryKey: [...KEY, params],
    queryFn: () => transactionsApi.searchTransactions(params),
    placeholderData: keepPreviousData,
    staleTime: 30_000,
  });
}

export function useTransactionDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => transactionsApi.getTransaction(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

/** Server-side pricing of the cart (promotions, discount, VAT). Nothing is saved. */
export function useCartPreview(
  storeId: string,
  items: { productId: string; quantity: number }[],
) {
  return useQuery({
    queryKey: [...KEY, 'preview', storeId, items],
    queryFn: () => transactionsApi.previewCart(storeId, items),
    enabled: !!storeId && items.length > 0,
    placeholderData: keepPreviousData,
    staleTime: 15_000,
  });
}

export function useVoidTransaction() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      transactionsApi.voidTransaction(id, reason),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useCreateReturn() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: BackofficeReturnPayload }) =>
      transactionsApi.createReturn(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
