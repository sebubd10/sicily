import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as manufacturersApi from '../api/manufacturersApi';
import type { ManufacturerFormData } from '../types/manufacturer';

const KEY = ['manufacturers'] as const;

export function useManufacturers() {
  return useQuery({
    queryKey: KEY,
    queryFn: manufacturersApi.getManufacturers,
    staleTime: 30_000,
  });
}

export function useManufacturerDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => manufacturersApi.getManufacturerById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useCreateManufacturer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: ManufacturerFormData) => manufacturersApi.createManufacturer(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdateManufacturer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: string; form: ManufacturerFormData }) =>
      manufacturersApi.updateManufacturer(id, form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useActivateManufacturer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => manufacturersApi.activateManufacturer(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeactivateManufacturer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => manufacturersApi.deactivateManufacturer(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
