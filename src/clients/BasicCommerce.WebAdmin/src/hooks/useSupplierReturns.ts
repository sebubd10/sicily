import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import * as srApi from '../api/supplierReturnsApi';
import type { CreateSupplierReturnFormData, AddSupplierReturnItemFormData } from '../types/supplierReturn';

const KEY = ['supplier-returns'] as const;

export function useSupplierReturns(params: {
  page: number;
  pageSize: number;
  status?: string;
  supplierId?: string;
  storeId?: string;
}) {
  return useQuery({
    queryKey: [...KEY, params],
    queryFn: () => srApi.getSupplierReturns(params),
    placeholderData: keepPreviousData,
    staleTime: 15_000,
  });
}

export function useSupplierReturnDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => srApi.getSupplierReturnById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useCreateSupplierReturn() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: CreateSupplierReturnFormData) => srApi.createSupplierReturn(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useAddSupplierReturnItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, item }: { id: string; item: AddSupplierReturnItemFormData }) =>
      srApi.addSupplierReturnItem(id, item),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useRemoveSupplierReturnItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, productId }: { id: string; productId: string }) =>
      srApi.removeSupplierReturnItem(id, productId),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useSetExpectedCredit() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, amount }: { id: string; amount: number }) =>
      srApi.setExpectedCredit(id, amount),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useSubmitSupplierReturn() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => srApi.submitSupplierReturn(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useShipSupplierReturn() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => srApi.shipSupplierReturn(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useReceiveCredit() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      creditAmount,
      creditNoteReference,
    }: {
      id: string;
      creditAmount: number;
      creditNoteReference?: string;
    }) => srApi.receiveCredit(id, creditAmount, creditNoteReference),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useCancelSupplierReturn() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => srApi.cancelSupplierReturn(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
