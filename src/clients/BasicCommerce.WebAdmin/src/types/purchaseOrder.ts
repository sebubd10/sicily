export type PurchaseOrderStatus =
  | 'Draft'
  | 'Submitted'
  | 'PartiallyReceived'
  | 'Received'
  | 'Cancelled';

export interface PurchaseOrderItem {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  isProductActive: boolean;
  orderedQuantity: number;
  receivedQuantity: number;
  remainingQuantity: number;
  unitCost: number;
  totalCost: number;
  isFullyReceived: boolean;
}

export interface PurchaseOrderSummary {
  id: string;
  orderNumber: string;
  supplierId: string;
  supplierName: string;
  warehouseId: string;
  warehouseName: string;
  status: PurchaseOrderStatus;
  orderDate: string;
  expectedDate: string | null;
  currency: string;
  totalAmount: number;
  itemsCount: number;
}

export interface PurchaseOrderDetail extends PurchaseOrderSummary {
  receivedDate: string | null;
  notes: string | null;
  items: PurchaseOrderItem[];
}

export interface PurchaseOrderListResponse {
  items: PurchaseOrderSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreatePOLineItem {
  productId: string;
  productName: string;
  sku: string;
  isProductActive?: boolean;
  quantity: number;
  unitCost: number;
}

export interface CreatePurchaseOrderFormData {
  supplierId: string;
  warehouseId: string;
  orderDate: string;
  expectedDate: string;
  notes: string;
  currency: string;
  items: CreatePOLineItem[];
}

export interface ReceiveLineItem {
  productId: string;
  receivedQuantity: number;
}

export const PO_STATUS_CONFIG: Record<
  PurchaseOrderStatus,
  { label: string; color: string; dot: string }
> = {
  Draft:             { label: 'Draft',              color: 'bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400',               dot: 'bg-gray-400' },
  Submitted:         { label: 'Submitted',           color: 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300',             dot: 'bg-blue-500' },
  PartiallyReceived: { label: 'Partially Received',  color: 'bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300',         dot: 'bg-amber-500' },
  Received:          { label: 'Received',            color: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300', dot: 'bg-emerald-500' },
  Cancelled:         { label: 'Cancelled',           color: 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',                dot: 'bg-red-400' },
};
