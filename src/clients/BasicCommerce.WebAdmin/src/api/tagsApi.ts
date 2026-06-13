import { api } from './axiosInstance';
import type { TagDetail, TagListResponse, TagListParams, TagFormData } from '../types/tag';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getTags(params: TagListParams): Promise<TagListResponse> {
  const { data } = await api.get<ApiResponse<TagListResponse>>('/product-tags', {
    params: {
      page: params.page,
      pageSize: params.pageSize,
      search: params.search || undefined,
      status: params.includeInactive ? undefined : 'Active',
    },
  });
  const res = data.data!;
  return {
    ...res,
    totalPages: res.pageSize > 0 ? Math.ceil(res.totalCount / res.pageSize) : 0,
  };
}

export async function getTagById(id: string): Promise<TagDetail> {
  const { data } = await api.get<ApiResponse<TagDetail>>(`/product-tags/${id}`);
  return data.data!;
}

export async function createTag(form: TagFormData): Promise<TagDetail> {
  const { data } = await api.post<ApiResponse<TagDetail>>('/product-tags', { name: form.name });
  return data.data!;
}

export async function updateTag(id: string, form: TagFormData): Promise<TagDetail> {
  const { data } = await api.put<ApiResponse<TagDetail>>(`/product-tags/${id}`, { name: form.name });
  return data.data!;
}

export async function deleteTag(id: string): Promise<void> {
  await api.delete(`/product-tags/${id}`);
}

export async function activateTag(id: string): Promise<void> {
  await api.put(`/product-tags/${id}/activate`);
}

export async function deactivateTag(id: string): Promise<void> {
  await api.put(`/product-tags/${id}/deactivate`);
}

export async function bulkDeleteTags(ids: string[]): Promise<void> {
  await api.post('/product-tags/bulk-delete', { ids });
}
