export interface Warehouse {
  id: string;
  name: string;
  code: string;
  phone: string | null;
  email: string | null;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  district: string;
  postalCode: string;
  country: string;
  isDefault: boolean;
  status: string;
}

export interface WarehouseFormData {
  name: string;
  code: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  district: string;
  postalCode: string;
  country: string;
  phone: string;
  email: string;
  isDefault: boolean;
}

export interface WarehouseStockLevel {
  warehouseId: string;
  warehouseName: string;
  productId: string;
  productName: string;
  sku: string;
  barcode: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  lowStockThreshold: number;
  isLowStock: boolean;
}

export interface WarehouseMovement {
  id: string;
  movementType: string;
  productId: string;
  productName: string;
  productSku: string;
  quantity: number;
  quantityBefore: number;
  quantityAfter: number;
  relatedStoreId: string | null;
  purchaseOrderId: string | null;
  reference: string | null;
  notes: string | null;
  recordedByUserId: string;
  createdAt: string;
}

export interface Store {
  id: string;
  name: string;
  code: string;
  address: string;
  status: string;
}

export interface TransferFormData {
  storeId: string;
  quantity: string;
  notes: string;
}

export interface District {
  code: string;
  name: string;
}
