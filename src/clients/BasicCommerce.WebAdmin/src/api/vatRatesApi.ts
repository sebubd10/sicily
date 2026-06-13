import { api } from './axiosInstance';
import type { VatRate, VatRateFormData } from '../types/vatRate';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getVatRates(): Promise<VatRate[]> {
  const { data } = await api.get<ApiResponse<VatRate[]>>('/vat-rates');
  return data.data!;
}

export async function getVatRateById(id: string): Promise<VatRate> {
  const { data } = await api.get<ApiResponse<VatRate>>(`/vat-rates/${id}`);
  return data.data!;
}

export async function createVatRate(form: VatRateFormData): Promise<VatRate> {
  const { data } = await api.post<ApiResponse<VatRate>>('/vat-rates', {
    name: form.name,
    code: form.code,
    rate: Number(form.rate),
    isDefault: form.isDefault,
  });
  return data.data!;
}

export async function updateVatRate(id: string, form: VatRateFormData): Promise<VatRate> {
  const { data } = await api.put<ApiResponse<VatRate>>(`/vat-rates/${id}`, {
    name: form.name,
    code: form.code,
    rate: Number(form.rate),
    isDefault: form.isDefault,
  });
  return data.data!;
}

export async function activateVatRate(id: string): Promise<void> {
  await api.put(`/vat-rates/${id}/activate`);
}

export async function deactivateVatRate(id: string): Promise<void> {
  await api.put(`/vat-rates/${id}/deactivate`);
}

export async function deleteVatRate(id: string): Promise<void> {
  await api.delete(`/vat-rates/${id}`);
}
