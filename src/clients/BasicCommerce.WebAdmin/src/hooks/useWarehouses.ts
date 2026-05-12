import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as warehousesApi from '../api/warehousesApi';
import type { WarehouseFormData } from '../types/warehouse';

const KEY  = ['warehouses'] as const;
const STORES_KEY = ['stores'] as const;

export function useWarehouses() {
  return useQuery({
    queryKey: KEY,
    queryFn: warehousesApi.getWarehouses,
    staleTime: 30_000,
  });
}

export function useWarehouseDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => warehousesApi.getWarehouseById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useWarehouseStock(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'stock', id],
    queryFn: () => warehousesApi.getWarehouseStock(id!),
    enabled: !!id,
    staleTime: 30_000,
  });
}

export function useWarehouseMovements(
  id: string | null,
  params?: { from?: string; to?: string; limit?: number },
) {
  return useQuery({
    queryKey: [...KEY, 'movements', id, params],
    queryFn: () => warehousesApi.getWarehouseMovements(id!, params),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useStores() {
  return useQuery({
    queryKey: STORES_KEY,
    queryFn: warehousesApi.getStores,
    staleTime: 5 * 60_000,
  });
}

export function useCreateWarehouse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: WarehouseFormData) => warehousesApi.createWarehouse(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdateWarehouse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: string; form: WarehouseFormData }) =>
      warehousesApi.updateWarehouse(id, form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useActivateWarehouse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => warehousesApi.activateWarehouse(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeactivateWarehouse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => warehousesApi.deactivateWarehouse(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useTransferToStore() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: {
      warehouseId: string;
      storeId: string;
      productId: string;
      quantity: number;
      notes?: string;
    }) => warehousesApi.transferToStore(
      args.warehouseId, args.storeId, args.productId, args.quantity, args.notes,
    ),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: KEY });
      qc.invalidateQueries({ queryKey: [...KEY, 'stock', vars.warehouseId] });
    },
  });
}
