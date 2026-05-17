import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import * as categoriesApi from '../api/categoriesApi';
import type { CategoryFormData, CategoryListParams } from '../types/category';

const KEY = ['categories'] as const;

/** Paginated table data — keeps previous data while fetching next page. */
export function useCategories(params: CategoryListParams) {
  return useQuery({
    queryKey: [...KEY, params],
    queryFn: () => categoriesApi.getCategories(params),
    placeholderData: keepPreviousData,
  });
}

/** All active categories — used for the parent dropdown in the modal. */
export function useAllActiveCategories() {
  return useQuery({
    queryKey: [...KEY, 'all-active'],
    queryFn: () => categoriesApi.getCategories({ page: 1, pageSize: 500, includeInactive: false }),
    staleTime: 60_000,
    select: (data) => data.items,
  });
}

/** Fetch a single category by ID — only runs when id is non-null. */
export function useCategoryDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => categoriesApi.getCategoryById(id!),
    enabled: !!id,
    staleTime: 0,        // always re-fetch when opening the edit modal
  });
}

export function useCreateCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: CategoryFormData) => categoriesApi.createCategory(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdateCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: string; form: CategoryFormData }) =>
      categoriesApi.updateCategory(id, form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useActivateCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => categoriesApi.activateCategory(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeactivateCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => categoriesApi.deactivateCategory(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeleteCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => categoriesApi.deleteCategory(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
