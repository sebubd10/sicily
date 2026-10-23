import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import * as transactionsApi from '../api/transactionsApi';
import type { BackofficeReturnPayload, ExchangePayload, Transaction } from '../types/transaction';

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

export function useTransactionHistory(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'history', id],
    queryFn: () => transactionsApi.getTransactionHistory(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

/**
 * Wraps any API call that returns the updated transaction: puts the result straight into
 * the detail cache and refreshes everything that depends on it (list, history, points).
 */
export function useTransactionMutation<TArgs>(fn: (args: TArgs) => Promise<Transaction>) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: fn,
    onSuccess: (txn) => {
      qc.setQueryData([...KEY, 'detail', txn.id], txn);
      qc.invalidateQueries({ queryKey: KEY });
      qc.invalidateQueries({ queryKey: ['reward-points'] });
      qc.invalidateQueries({ queryKey: ['customers'] });
    },
  });
}

export function useExchangeTransaction() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: ExchangePayload }) =>
      transactionsApi.exchangeTransaction(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEY });
      qc.invalidateQueries({ queryKey: ['reward-points'] });
    },
  });
}
