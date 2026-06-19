export interface StockLevel {
  productId: string;
  productName: string;
  sku: string;
  barcode: string;
  categoryName: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  lowStockThreshold: number;
  isLowStock: boolean;
  isOutOfStock: boolean;
  lastCountedAt: string | null;
}

export interface StockMovement {
  id: string;
  type: string;
  productId: string;
  productName: string;
  productSku: string;
  quantity: number;
  quantityBefore: number;
  quantityAfter: number;
  reference: string | null;
  notes: string | null;
  relatedStoreId: string | null;
  relatedStoreName: string | null;
  recordedByUserId: string;
  recordedByName: string;
  createdAt: string;
}

export type StockMovementType =
  | 'Receive'
  | 'Adjustment'
  | 'WriteOff'
  | 'TransferOut'
  | 'TransferIn'
  | 'Sale'
  | 'Return'
  | 'ExpiryWriteOff'
  | 'SupplierReturn';

export const MOVEMENT_TYPE_CONFIG: Record<
  StockMovementType,
  { label: string; color: string; dot: string; direction: 1 | -1 | 0 }
> = {
  Receive:       { label: 'Receive',         color: 'bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400', dot: 'bg-emerald-500',  direction:  1 },
  Adjustment:    { label: 'Adjustment',      color: 'bg-blue-50 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',           dot: 'bg-blue-500',    direction:  0 },
  WriteOff:      { label: 'Write-off',       color: 'bg-red-50 text-red-700 dark:bg-red-900/30 dark:text-red-400',               dot: 'bg-red-500',     direction: -1 },
  TransferOut:   { label: 'Transfer Out',    color: 'bg-orange-50 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400',   dot: 'bg-orange-500',  direction: -1 },
  TransferIn:    { label: 'Transfer In',     color: 'bg-purple-50 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400',   dot: 'bg-purple-500',  direction:  1 },
  Sale:          { label: 'Sale',            color: 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',             dot: 'bg-gray-400',    direction: -1 },
  Return:        { label: 'Return',          color: 'bg-teal-50 text-teal-700 dark:bg-teal-900/30 dark:text-teal-400',           dot: 'bg-teal-500',    direction:  1 },
  ExpiryWriteOff:{ label: 'Expiry Write-off',color: 'bg-red-50 text-red-700 dark:bg-red-900/30 dark:text-red-400',               dot: 'bg-red-600',     direction: -1 },
  SupplierReturn:{ label: 'Supplier Return', color: 'bg-amber-50 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400',       dot: 'bg-amber-500',   direction: -1 },
};

export const ALL_MOVEMENT_TYPES = Object.keys(MOVEMENT_TYPE_CONFIG) as StockMovementType[];

export type StockActionType = 'receive' | 'adjust' | 'writeOff' | 'threshold';

// ── Stock Batches ─────────────────────────────────────────────────────────────

export interface StockBatch {
  id: string;
  storeId: string;
  productId: string;
  productName: string;
  productSku: string;
  lotNumber: string | null;
  expiryDate: string | null;
  receivedQuantity: number;
  remainingQuantity: number;
  unitCost: number | null;
  isExpired: boolean;
  createdAt: string;
}

export interface StockBatchListResponse {
  items: StockBatch[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ReceiveBatchRequest {
  productId: string;
  quantity: number;
  expiryDate?: string | null;
  lotNumber?: string | null;
  unitCost?: number | null;
  reference?: string | null;
  notes?: string | null;
}

export type BatchStatus = 'active' | 'expiring' | 'expired' | 'depleted';

export function getBatchStatus(batch: StockBatch): BatchStatus {
  if (batch.isExpired) return 'expired';
  if (batch.remainingQuantity <= 0) return 'depleted';
  if (batch.expiryDate) {
    const msUntilExpiry = new Date(batch.expiryDate).getTime() - Date.now();
    if (msUntilExpiry <= 0) return 'expired';
    if (msUntilExpiry <= 7 * 24 * 60 * 60 * 1000) return 'expiring';
  }
  return 'active';
}
