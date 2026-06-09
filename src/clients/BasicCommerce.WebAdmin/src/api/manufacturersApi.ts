import { api } from './axiosInstance';
import type { Manufacturer, ManufacturerFormData, Country } from '../types/manufacturer';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getManufacturers(): Promise<Manufacturer[]> {
  const { data } = await api.get<ApiResponse<Manufacturer[]>>('/manufacturers');
  return data.data!;
}

export async function getManufacturerById(id: string): Promise<Manufacturer> {
  const { data } = await api.get<ApiResponse<Manufacturer>>(`/manufacturers/${id}`);
  return data.data!;
}

export async function createManufacturer(form: ManufacturerFormData): Promise<Manufacturer> {
  const { data } = await api.post<ApiResponse<Manufacturer>>('/manufacturers', {
    name: form.name,
    code: form.code || null,
    country: form.country || null,
    website: form.website || null,
    contactEmail: form.contactEmail || null,
    notes: form.notes || null,
  });
  return data.data!;
}

export async function updateManufacturer(id: string, form: ManufacturerFormData): Promise<Manufacturer> {
  const { data } = await api.put<ApiResponse<Manufacturer>>(`/manufacturers/${id}`, {
    name: form.name,
    code: form.code || null,
    country: form.country || null,
    website: form.website || null,
    contactEmail: form.contactEmail || null,
    notes: form.notes || null,
  });
  return data.data!;
}

export async function activateManufacturer(id: string): Promise<void> {
  await api.put(`/manufacturers/${id}/activate`);
}

export async function deactivateManufacturer(id: string): Promise<void> {
  await api.put(`/manufacturers/${id}/deactivate`);
}

export async function deleteManufacturer(id: string): Promise<void> {
  await api.delete(`/manufacturers/${id}`);
}

export async function exportManufacturersPdf(params?: {
  search?: string;
  includeInactive?: boolean;
}): Promise<void> {
  const response = await api.get('/reports/manufacturers/pdf', {
    params: { search: params?.search, includeInactive: params?.includeInactive },
    responseType: 'blob',
  });
  const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
  const a = document.createElement('a');
  a.href = url;
  a.download = `manufacturers_${new Date().toISOString().slice(0, 10)}.pdf`;
  a.click();
  URL.revokeObjectURL(url);
}

export async function getCountries(): Promise<Country[]> {
  const { data } = await api.get<ApiResponse<Country[]>>('/manufacturers/countries');
  return data.data!;
}
