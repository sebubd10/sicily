import { api } from './axiosInstance';
import type { Store, StoreFormData } from '../types/store';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getStores(): Promise<Store[]> {
  const { data } = await api.get<ApiResponse<Store[]>>('/stores');
  return data.data!;
}

export async function getStoreById(id: string): Promise<Store> {
  const { data } = await api.get<ApiResponse<Store>>(`/stores/${id}`);
  return data.data!;
}

export async function createStore(form: StoreFormData): Promise<Store> {
  const { data } = await api.post<ApiResponse<Store>>('/stores', {
    name: form.name,
    code: form.code,
    addressLine1: form.addressLine1,
    city: form.city,
    district: form.district,
    postalCode: form.postalCode,
    phone: form.phone || null,
    email: form.email || null,
  });
  return data.data!;
}

export async function updateStore(id: string, form: StoreFormData): Promise<Store> {
  const { data } = await api.put<ApiResponse<Store>>(`/stores/${id}`, {
    name: form.name,
    addressLine1: form.addressLine1,
    addressLine2: form.addressLine2 || null,
    city: form.city,
    district: form.district,
    postalCode: form.postalCode,
    phone: form.phone || null,
    email: form.email || null,
    openingTime: form.openingTime,
    closingTime: form.closingTime,
  });
  return data.data!;
}

export async function activateStore(id: string): Promise<void> {
  await api.put(`/stores/${id}/activate`);
}

export async function deactivateStore(id: string): Promise<void> {
  await api.put(`/stores/${id}/deactivate`);
}
