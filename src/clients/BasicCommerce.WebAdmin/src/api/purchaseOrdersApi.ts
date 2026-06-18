import { api } from './axiosInstance';
import type {
  PurchaseOrderDetail,
  PurchaseOrderListResponse,
  CreatePurchaseOrderFormData,
} from '../types/purchaseOrder';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getPurchaseOrders(params: {
  page?: number;
  pageSize?: number;
  status?: string;
  supplierId?: string;
  warehouseId?: string;
  from?: string;
  to?: string;
  dateField?: string;
}): Promise<PurchaseOrderListResponse> {
  const { data } = await api.get<ApiResponse<PurchaseOrderListResponse>>('/purchase-orders', {
    params: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
      status: params.status || undefined,
      supplierId: params.supplierId || undefined,
      warehouseId: params.warehouseId || undefined,
      from: params.from || undefined,
      to: params.to || undefined,
      dateField: params.dateField || undefined,
    },
  });
  return data.data!;
}

export async function getPurchaseOrderById(id: string): Promise<PurchaseOrderDetail> {
  const { data } = await api.get<ApiResponse<PurchaseOrderDetail>>(`/purchase-orders/${id}`);
  return data.data!;
}

export async function createPurchaseOrder(
  form: CreatePurchaseOrderFormData,
): Promise<PurchaseOrderDetail> {
  const { data } = await api.post<ApiResponse<PurchaseOrderDetail>>('/purchase-orders', {
    supplierId: form.supplierId,
    warehouseId: form.warehouseId,
    orderDate: form.orderDate,
    expectedDate: form.expectedDate || null,
    notes: form.notes || null,
    currency: form.currency,
    items: form.items.map((i) => ({
      productId: i.productId,
      quantity: i.quantity,
      unitCost: i.unitCost,
    })),
  });
  return data.data!;
}

export async function updatePurchaseOrder(
  id: string,
  form: CreatePurchaseOrderFormData,
): Promise<PurchaseOrderDetail> {
  const { data } = await api.put<ApiResponse<PurchaseOrderDetail>>(`/purchase-orders/${id}`, {
    supplierId: form.supplierId,
    warehouseId: form.warehouseId,
    orderDate: form.orderDate,
    expectedDate: form.expectedDate || null,
    notes: form.notes || null,
    currency: form.currency,
    items: form.items.map((i) => ({
      productId: i.productId,
      quantity: i.quantity,
      unitCost: i.unitCost,
    })),
  });
  return data.data!;
}

export async function submitPurchaseOrder(id: string): Promise<PurchaseOrderDetail> {
  const { data } = await api.put<ApiResponse<PurchaseOrderDetail>>(`/purchase-orders/${id}/submit`);
  return data.data!;
}

export async function cancelPurchaseOrder(id: string): Promise<void> {
  await api.put(`/purchase-orders/${id}/cancel`);
}

export async function receivePurchaseOrder(
  id: string,
  items: { productId: string; receivedQuantity: number }[],
  notes?: string,
): Promise<PurchaseOrderDetail> {
  const { data } = await api.post<ApiResponse<PurchaseOrderDetail>>(
    `/purchase-orders/${id}/receive`,
    { items, notes: notes || null },
  );
  return data.data!;
}
