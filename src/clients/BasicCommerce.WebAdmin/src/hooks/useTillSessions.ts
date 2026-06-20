import { useQuery } from '@tanstack/react-query';
import * as tillSessionApi from '../api/tillSessionApi';

const KEY = ['till-sessions'] as const;

export function useTillSessions(params: {
  storeId?: string;
  terminalId?: string;
  openOnly?: boolean;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: [...KEY, params],
    queryFn: () => tillSessionApi.getTillSessions(params),
    staleTime: 30_000,
  });
}

export function useTillSession(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => tillSessionApi.getTillSession(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useXReport(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'x-report', id],
    queryFn: () => tillSessionApi.getXReport(id!),
    enabled: !!id,
    staleTime: 0,
  });
}
