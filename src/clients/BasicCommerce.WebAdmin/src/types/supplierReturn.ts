export type SupplierReturnStatus =
  | 'Draft'
  | 'Submitted'
  | 'Shipped'
  | 'CreditReceived'
  | 'Cancelled';

export type SupplierReturnReason =
  | 'Damaged'
  | 'Defective'
  | 'WrongItemShipped'
  | 'Overstock'
  | 'QualityIssue'
  | 'Other';

export interface SupplierReturnItem {
  id: string;
  productId: string;
  productName: string;
  productSku: string;
  quantity: number;
  unitCost: number;
  totalCost: number;
  reason: string;
  notes: string | null;
}

export interface SupplierReturnDetail {
  id: string;
  returnNumber: string;
  supplierId: string;
  supplierName: string;
  storeId: string;
  purchaseOrderId: string | null;
  status: SupplierReturnStatus;
  notes: string | null;
  totalReturnValue: number;
  expectedCreditAmount: number | null;
  actualCreditAmount: number | null;
  creditNoteReference: string | null;
  shippedAt: string | null;
  creditReceivedAt: string | null;
  items: SupplierReturnItem[];
  createdAt: string;
  updatedAt: string | null;
}

export interface SupplierReturnSummary {
  id: string;
  returnNumber: string;
  supplierId: string;
  supplierName: string;
  storeId: string;
  status: SupplierReturnStatus;
  totalReturnValue: number;
  expectedCreditAmount: number | null;
  actualCreditAmount: number | null;
  createdAt: string;
  shippedAt: string | null;
}

export interface SupplierReturnListResponse {
  items: SupplierReturnSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateSupplierReturnFormData {
  supplierId: string;
  storeId: string;
  purchaseOrderId?: string;
  notes?: string;
}

export interface AddSupplierReturnItemFormData {
  productId: string;
  quantity: number;
  unitCost: number;
  reason: SupplierReturnReason;
  notes?: string;
}

export const RETURN_STATUS_CONFIG: Record<
  SupplierReturnStatus,
  { label: string; color: string; dot: string; step: number }
> = {
  Draft:         { label: 'Draft',          color: 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',         dot: 'bg-gray-400',    step: 1 },
  Submitted:     { label: 'Submitted',      color: 'bg-blue-50 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',       dot: 'bg-blue-500',    step: 2 },
  Shipped:       { label: 'Shipped',        color: 'bg-purple-50 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400', dot: 'bg-purple-500', step: 3 },
  CreditReceived:{ label: 'Credit Received',color: 'bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400', dot: 'bg-emerald-500', step: 4 },
  Cancelled:     { label: 'Cancelled',      color: 'bg-red-50 text-red-700 dark:bg-red-900/30 dark:text-red-400',           dot: 'bg-red-500',     step: 0 },
};

export const RETURN_REASONS: { value: SupplierReturnReason; label: string }[] = [
  { value: 'Damaged',         label: 'Damaged' },
  { value: 'Defective',       label: 'Defective' },
  { value: 'WrongItemShipped',label: 'Wrong Item Shipped' },
  { value: 'Overstock',       label: 'Overstock' },
  { value: 'QualityIssue',    label: 'Quality Issue' },
  { value: 'Other',           label: 'Other' },
];

export function reasonLabel(r: string): string {
  return RETURN_REASONS.find((x) => x.value === r)?.label ?? r;
}
