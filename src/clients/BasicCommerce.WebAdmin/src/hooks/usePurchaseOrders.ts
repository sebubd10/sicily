import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import * as poApi from '../api/purchaseOrdersApi';
import type { CreatePurchaseOrderFormData } from '../types/purchaseOrder';

const KEY = ['purchase-orders'] as const;

export function usePurchaseOrders(params: {
  page: number;
  pageSize: number;
  status?: string;
  supplierId?: string;
  warehouseId?: string;
  from?: string;
  to?: string;
  dateField?: string;
}) {
  return useQuery({
    queryKey: [...KEY, params],
    queryFn: () => poApi.getPurchaseOrders(params),
    placeholderData: keepPreviousData,
    staleTime: 15_000,
  });
}

export function usePurchaseOrderDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => poApi.getPurchaseOrderById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useCreatePurchaseOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: CreatePurchaseOrderFormData) => poApi.createPurchaseOrder(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdatePurchaseOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: string; form: CreatePurchaseOrderFormData }) =>
      poApi.updatePurchaseOrder(id, form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useSubmitPurchaseOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => poApi.submitPurchaseOrder(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useCancelPurchaseOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => poApi.cancelPurchaseOrder(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useReceivePurchaseOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      items,
      notes,
    }: {
      id: string;
      items: { productId: string; receivedQuantity: number }[];
      notes?: string;
    }) => poApi.receivePurchaseOrder(id, items, notes),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
