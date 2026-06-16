import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as suppliersApi from '../api/suppliersApi';
import type { SupplierFormData } from '../types/supplier';

const KEY = ['suppliers'] as const;

export function useSuppliers() {
  return useQuery({
    queryKey: KEY,
    queryFn: suppliersApi.getSuppliers,
    staleTime: 30_000,
  });
}

export function useSupplierDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => suppliersApi.getSupplierById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useCreateSupplier() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: SupplierFormData) => suppliersApi.createSupplier(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdateSupplier() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: string; form: SupplierFormData }) =>
      suppliersApi.updateSupplier(id, form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useActivateSupplier() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => suppliersApi.activateSupplier(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeactivateSupplier() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => suppliersApi.deactivateSupplier(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeleteSupplier() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => suppliersApi.deleteSupplier(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
