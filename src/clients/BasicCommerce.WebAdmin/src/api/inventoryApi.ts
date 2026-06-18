import { api } from './axiosInstance';
import type { StockLevel, StockMovement } from '../types/inventory';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

// ── Stock Levels ──────────────────────────────────────────────────────────────

export async function getStockLevels(storeId: string): Promise<StockLevel[]> {
  const { data } = await api.get<ApiResponse<StockLevel[]>>(`/inventory/${storeId}/stock`);
  return data.data!;
}

export async function getLowStock(storeId: string): Promise<StockLevel[]> {
  const { data } = await api.get<ApiResponse<StockLevel[]>>(`/inventory/${storeId}/low-stock`);
  return data.data!;
}

// ── Stock Movements ───────────────────────────────────────────────────────────

export async function getStockMovements(
  storeId: string,
  params?: { productId?: string; from?: string; to?: string; limit?: number },
): Promise<StockMovement[]> {
  const { data } = await api.get<ApiResponse<StockMovement[]>>(
    `/inventory/${storeId}/movements`,
    {
      params: {
        productId: params?.productId || undefined,
        from: params?.from || undefined,
        to: params?.to || undefined,
        limit: params?.limit ?? 100,
      },
    },
  );
  return data.data!;
}

// ── Mutations ─────────────────────────────────────────────────────────────────

export async function receiveStock(
  storeId: string,
  productId: string,
  quantity: number,
  reference?: string,
  notes?: string,
): Promise<StockLevel> {
  const { data } = await api.post<ApiResponse<StockLevel>>(`/inventory/${storeId}/receive`, {
    productId,
    quantity,
    reference: reference || null,
    notes: notes || null,
  });
  return data.data!;
}

export async function adjustStock(
  storeId: string,
  productId: string,
  newQuantity: number,
  notes: string,
): Promise<StockLevel> {
  const { data } = await api.post<ApiResponse<StockLevel>>(`/inventory/${storeId}/adjust`, {
    productId,
    newQuantity,
    notes,
  });
  return data.data!;
}

export async function writeOffStock(
  storeId: string,
  productId: string,
  quantity: number,
  reason: string,
): Promise<StockLevel> {
  const { data } = await api.post<ApiResponse<StockLevel>>(`/inventory/${storeId}/write-off`, {
    productId,
    quantity,
    reason,
  });
  return data.data!;
}

export async function transferStock(
  sourceStoreId: string,
  destinationStoreId: string,
  productId: string,
  quantity: number,
  notes?: string,
): Promise<StockLevel> {
  const { data } = await api.post<ApiResponse<StockLevel>>('/inventory/transfer', {
    sourceStoreId,
    destinationStoreId,
    productId,
    quantity,
    notes: notes || null,
  });
  return data.data!;
}

export async function setLowStockThreshold(
  storeId: string,
  productId: string,
  threshold: number,
): Promise<StockLevel> {
  const { data } = await api.put<ApiResponse<StockLevel>>(`/inventory/${storeId}/threshold`, {
    productId,
    threshold,
  });
  return data.data!;
}
