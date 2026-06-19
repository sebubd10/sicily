import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as inventoryApi from '../api/inventoryApi';
import type { ReceiveBatchRequest } from '../types/inventory';

const STOCK_KEY     = ['inventory', 'stock'] as const;
const MOVEMENTS_KEY = ['inventory', 'movements'] as const;
const BATCHES_KEY   = ['inventory', 'batches'] as const;

// ── Queries ───────────────────────────────────────────────────────────────────

export function useStockLevels(storeId: string | null) {
  return useQuery({
    queryKey: [...STOCK_KEY, storeId],
    queryFn: () => inventoryApi.getStockLevels(storeId!),
    enabled: !!storeId,
    staleTime: 30_000,
  });
}

export function useStockMovements(
  storeId: string | null,
  params?: { productId?: string; from?: string; to?: string; limit?: number },
) {
  return useQuery({
    queryKey: [...MOVEMENTS_KEY, storeId, params],
    queryFn: () => inventoryApi.getStockMovements(storeId!, params),
    enabled: !!storeId,
    staleTime: 0,
  });
}

// ── Mutations ─────────────────────────────────────────────────────────────────

export function useReceiveStock() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: {
      storeId: string;
      productId: string;
      quantity: number;
      reference?: string;
      notes?: string;
    }) => inventoryApi.receiveStock(
      args.storeId, args.productId, args.quantity, args.reference, args.notes,
    ),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: [...STOCK_KEY, vars.storeId] });
      qc.invalidateQueries({ queryKey: [...MOVEMENTS_KEY, vars.storeId] });
    },
  });
}

export function useAdjustStock() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: {
      storeId: string;
      productId: string;
      newQuantity: number;
      notes: string;
    }) => inventoryApi.adjustStock(args.storeId, args.productId, args.newQuantity, args.notes),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: [...STOCK_KEY, vars.storeId] });
      qc.invalidateQueries({ queryKey: [...MOVEMENTS_KEY, vars.storeId] });
    },
  });
}

export function useWriteOffStock() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: {
      storeId: string;
      productId: string;
      quantity: number;
      reason: string;
    }) => inventoryApi.writeOffStock(args.storeId, args.productId, args.quantity, args.reason),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: [...STOCK_KEY, vars.storeId] });
      qc.invalidateQueries({ queryKey: [...MOVEMENTS_KEY, vars.storeId] });
    },
  });
}

export function useTransferStock() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: {
      sourceStoreId: string;
      destinationStoreId: string;
      productId: string;
      quantity: number;
      notes?: string;
    }) => inventoryApi.transferStock(
      args.sourceStoreId, args.destinationStoreId, args.productId, args.quantity, args.notes,
    ),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: [...STOCK_KEY, vars.sourceStoreId] });
      qc.invalidateQueries({ queryKey: [...STOCK_KEY, vars.destinationStoreId] });
      qc.invalidateQueries({ queryKey: MOVEMENTS_KEY });
    },
  });
}

export function useSetLowStockThreshold() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { storeId: string; productId: string; threshold: number }) =>
      inventoryApi.setLowStockThreshold(args.storeId, args.productId, args.threshold),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: [...STOCK_KEY, vars.storeId] });
    },
  });
}

// ── Stock Batch Hooks ─────────────────────────────────────────────────────────

export function useStockBatches(
  storeId: string | null,
  params?: { productId?: string; includeExpired?: boolean; page?: number; pageSize?: number },
) {
  return useQuery({
    queryKey: [...BATCHES_KEY, storeId, params],
    queryFn: () => inventoryApi.getStockBatches(storeId!, params),
    enabled: !!storeId,
    staleTime: 30_000,
  });
}

export function useExpiringBatches(storeId: string | null, withinDays = 7) {
  return useQuery({
    queryKey: [...BATCHES_KEY, storeId, 'expiring', withinDays],
    queryFn: () => inventoryApi.getExpiringBatches(storeId!, withinDays),
    enabled: !!storeId,
    staleTime: 60_000,
  });
}

export function useReceiveBatch() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { storeId: string; request: ReceiveBatchRequest }) =>
      inventoryApi.receiveBatch(args.storeId, args.request),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: [...BATCHES_KEY, vars.storeId] });
      qc.invalidateQueries({ queryKey: [...STOCK_KEY, vars.storeId] });
    },
  });
}

export function useExpireBatches() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { storeId: string; notes?: string }) =>
      inventoryApi.expireBatches(args.storeId, args.notes),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: [...BATCHES_KEY, vars.storeId] });
      qc.invalidateQueries({ queryKey: [...STOCK_KEY, vars.storeId] });
    },
  });
}

export function useDeleteStockBatch() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { batchId: string; storeId: string }) =>
      inventoryApi.deleteStockBatch(args.batchId),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: [...BATCHES_KEY, vars.storeId] });
    },
  });
}
