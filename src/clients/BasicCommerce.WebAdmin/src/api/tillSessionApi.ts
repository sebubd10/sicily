import { api } from './axiosInstance';
import type { TillSession, TillSessionListResponse, TillReport } from '../types/tillSession';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getTillSessions(params: {
  storeId?: string;
  terminalId?: string;
  openOnly?: boolean;
  page?: number;
  pageSize?: number;
}): Promise<TillSessionListResponse> {
  const { data } = await api.get<ApiResponse<TillSessionListResponse>>('/till-sessions', {
    params: {
      storeId: params.storeId || undefined,
      terminalId: params.terminalId || undefined,
      openOnly: params.openOnly ?? false,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  });
  return data.data!;
}

export async function getTillSession(id: string): Promise<TillSession> {
  const { data } = await api.get<ApiResponse<TillSession>>(`/till-sessions/${id}`);
  return data.data!;
}

export async function getXReport(id: string): Promise<TillReport> {
  const { data } = await api.get<ApiResponse<TillReport>>(`/till-sessions/${id}/x-report`);
  return data.data!;
}
