import { api } from './axiosInstance';
import type {
  Warehouse,
  WarehouseFormData,
  WarehouseStockLevel,
  WarehouseMovement,
  Store,
} from '../types/warehouse';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getWarehouses(): Promise<Warehouse[]> {
  const { data } = await api.get<ApiResponse<Warehouse[]>>('/warehouses');
  return data.data!;
}

export async function getWarehouseById(id: string): Promise<Warehouse> {
  const { data } = await api.get<ApiResponse<Warehouse>>(`/warehouses/${id}`);
  return data.data!;
}

export async function getWarehouseStock(id: string): Promise<WarehouseStockLevel[]> {
  const { data } = await api.get<ApiResponse<WarehouseStockLevel[]>>(`/warehouses/${id}/stock`);
  return data.data!;
}

export async function getWarehouseMovements(
  id: string,
  params?: { from?: string; to?: string; limit?: number },
): Promise<WarehouseMovement[]> {
  const { data } = await api.get<ApiResponse<WarehouseMovement[]>>(
    `/warehouses/${id}/movements`,
    { params: { from: params?.from, to: params?.to, limit: params?.limit ?? 200 } },
  );
  return data.data!;
}

export async function createWarehouse(form: WarehouseFormData): Promise<Warehouse> {
  const { data } = await api.post<ApiResponse<Warehouse>>('/warehouses', {
    name: form.name,
    code: form.code,
    addressLine1: form.addressLine1,
    addressLine2: form.addressLine2 || null,
    city: form.city,
    district: form.district,
    postalCode: form.postalCode,
    country: form.country || 'BD',
    phone: form.phone || null,
    email: form.email || null,
    isDefault: form.isDefault,
  });
  return data.data!;
}

export async function updateWarehouse(id: string, form: WarehouseFormData): Promise<Warehouse> {
  const { data } = await api.put<ApiResponse<Warehouse>>(`/warehouses/${id}`, {
    name: form.name,
    addressLine1: form.addressLine1,
    addressLine2: form.addressLine2 || null,
    city: form.city,
    district: form.district,
    postalCode: form.postalCode,
    country: form.country || 'BD',
    phone: form.phone || null,
    email: form.email || null,
  });
  return data.data!;
}

export async function activateWarehouse(id: string): Promise<void> {
  await api.put(`/warehouses/${id}/activate`);
}

export async function deactivateWarehouse(id: string): Promise<void> {
  await api.put(`/warehouses/${id}/deactivate`);
}

export async function transferToStore(
  warehouseId: string,
  storeId: string,
  productId: string,
  quantity: number,
  notes?: string,
): Promise<void> {
  await api.post('/warehouses/transfer-to-store', {
    warehouseId,
    storeId,
    productId,
    quantity,
    notes: notes || null,
  });
}

export async function getStores(): Promise<Store[]> {
  const { data } = await api.get<ApiResponse<Store[]>>('/stores');
  return data.data!;
}
