import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as storesApi from '../api/storesApi';
import type { StoreFormData } from '../types/store';

const KEY = ['stores-admin'] as const;
const DROPDOWN_KEY = ['stores'] as const;

export function useStoreList() {
  return useQuery({
    queryKey: KEY,
    queryFn: storesApi.getStores,
    staleTime: 30_000,
  });
}

export function useStoreDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => storesApi.getStoreById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

function invalidateAll(qc: ReturnType<typeof useQueryClient>) {
  qc.invalidateQueries({ queryKey: KEY });
  qc.invalidateQueries({ queryKey: DROPDOWN_KEY });
}

export function useCreateStore() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: StoreFormData) => storesApi.createStore(form),
    onSuccess: () => invalidateAll(qc),
  });
}

export function useUpdateStore() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: string; form: StoreFormData }) =>
      storesApi.updateStore(id, form),
    onSuccess: () => invalidateAll(qc),
  });
}

export function useActivateStore() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => storesApi.activateStore(id),
    onSuccess: () => invalidateAll(qc),
  });
}

export function useDeactivateStore() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => storesApi.deactivateStore(id),
    onSuccess: () => invalidateAll(qc),
  });
}
