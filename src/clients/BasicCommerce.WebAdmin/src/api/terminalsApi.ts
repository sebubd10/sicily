import { api } from './axiosInstance';
import type { Terminal, TerminalFormData } from '../types/terminal';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getTerminals(): Promise<Terminal[]> {
  const { data } = await api.get<ApiResponse<Terminal[]>>('/terminals');
  return data.data!;
}

export async function getTerminalById(id: string): Promise<Terminal> {
  const { data } = await api.get<ApiResponse<Terminal>>(`/terminals/${id}`);
  return data.data!;
}

export async function createTerminal(form: TerminalFormData): Promise<Terminal> {
  const { data } = await api.post<ApiResponse<Terminal>>('/terminals', {
    storeId: form.storeId,
    name: form.name,
    code: form.code,
    type: form.type,
  });
  return data.data!;
}

export async function updateTerminal(id: string, form: TerminalFormData): Promise<Terminal> {
  const { data } = await api.put<ApiResponse<Terminal>>(`/terminals/${id}`, {
    storeId: form.storeId,
    name: form.name,
    code: form.code,
    type: form.type,
  });
  return data.data!;
}

export async function activateTerminal(id: string): Promise<void> {
  await api.put(`/terminals/${id}/activate`);
}

export async function deactivateTerminal(id: string): Promise<void> {
  await api.put(`/terminals/${id}/deactivate`);
}

export async function deleteTerminal(id: string): Promise<void> {
  await api.delete(`/terminals/${id}`);
}
