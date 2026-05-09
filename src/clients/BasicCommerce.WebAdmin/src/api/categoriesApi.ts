import { api } from './axiosInstance';
import type {
  Category,
  CategoryFormData,
  CategoryListParams,
  PaginatedCategories,
} from '../types/category';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getCategories(params: CategoryListParams): Promise<PaginatedCategories> {
  const { data } = await api.get<ApiResponse<PaginatedCategories>>('/categories', {
    params: {
      pageNumber: params.page,
      pageSize: params.pageSize,
      search: params.search || undefined,
      includeInactive: params.includeInactive ?? false,
    },
  });
  return data.data!;
}

export async function getCategoryById(id: string): Promise<Category> {
  const { data } = await api.get<ApiResponse<Category>>(`/categories/${id}`);
  return data.data!;
}

export async function createCategory(form: CategoryFormData): Promise<Category> {
  const { data } = await api.post<ApiResponse<Category>>('/categories', {
    name: form.name,
    nameBn: form.nameBn,
    description: form.description || null,
    parentCategoryId: form.parentCategoryId || null,
  });
  return data.data!;
}

export async function updateCategory(id: string, form: CategoryFormData): Promise<Category> {
  const { data } = await api.put<ApiResponse<Category>>(`/categories/${id}`, {
    name: form.name,
    nameBn: form.nameBn,
    description: form.description || null,
    sortOrder: form.sortOrder,
    parentCategoryId: form.parentCategoryId || null,
  });
  return data.data!;
}

export async function activateCategory(id: string): Promise<void> {
  await api.put(`/categories/${id}/activate`);
}

export async function deactivateCategory(id: string): Promise<void> {
  await api.put(`/categories/${id}/deactivate`);
}

export async function deleteCategory(id: string): Promise<void> {
  await api.delete(`/categories/${id}`);
}
