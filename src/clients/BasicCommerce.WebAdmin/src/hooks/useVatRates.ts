import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as vatRatesApi from '../api/vatRatesApi';
import type { VatRateFormData } from '../types/vatRate';

const KEY = ['vat-rates'] as const;

export function useVatRates() {
  return useQuery({
    queryKey: KEY,
    queryFn: vatRatesApi.getVatRates,
    staleTime: 30_000,
  });
}

export function useVatRateDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => vatRatesApi.getVatRateById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useCreateVatRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: VatRateFormData) => vatRatesApi.createVatRate(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdateVatRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: string; form: VatRateFormData }) =>
      vatRatesApi.updateVatRate(id, form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useActivateVatRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => vatRatesApi.activateVatRate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeactivateVatRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => vatRatesApi.deactivateVatRate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeleteVatRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => vatRatesApi.deleteVatRate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
