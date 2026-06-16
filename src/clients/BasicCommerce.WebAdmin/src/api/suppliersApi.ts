import { api } from './axiosInstance';
import type { Supplier, SupplierFormData } from '../types/supplier';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getSuppliers(): Promise<Supplier[]> {
  const { data } = await api.get<ApiResponse<Supplier[]>>('/suppliers');
  return data.data!;
}

export async function getSupplierById(id: string): Promise<Supplier> {
  const { data } = await api.get<ApiResponse<Supplier>>(`/suppliers/${id}`);
  return data.data!;
}

export async function createSupplier(form: SupplierFormData): Promise<Supplier> {
  const { data } = await api.post<ApiResponse<Supplier>>('/suppliers', {
    name: form.name,
    code: form.code,
    contactName: form.contactName || null,
    email: form.email || null,
    phone: form.phone || null,
    addressLine1: form.addressLine1 || null,
    addressLine2: form.addressLine2 || null,
    city: form.city || null,
    district: form.district || null,
    postalCode: form.postalCode || null,
    leadTimeDays: form.leadTimeDays,
    notes: form.notes || null,
  });
  return data.data!;
}

export async function updateSupplier(id: string, form: SupplierFormData): Promise<Supplier> {
  const { data } = await api.put<ApiResponse<Supplier>>(`/suppliers/${id}`, {
    name: form.name,
    contactName: form.contactName || null,
    email: form.email || null,
    phone: form.phone || null,
    addressLine1: form.addressLine1 || null,
    addressLine2: form.addressLine2 || null,
    city: form.city || null,
    district: form.district || null,
    postalCode: form.postalCode || null,
    leadTimeDays: form.leadTimeDays,
    notes: form.notes || null,
  });
  return data.data!;
}

export async function activateSupplier(id: string): Promise<void> {
  await api.put(`/suppliers/${id}/activate`);
}

export async function deactivateSupplier(id: string): Promise<void> {
  await api.put(`/suppliers/${id}/deactivate`);
}

export async function deleteSupplier(id: string): Promise<void> {
  await api.delete(`/suppliers/${id}`);
}
