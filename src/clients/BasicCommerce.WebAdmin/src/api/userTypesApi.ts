import { api } from './axiosInstance';
import type { UserType, UserTypeFormData, MenuResponse, ApiPermission } from '../types/userType';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getUserTypes(): Promise<UserType[]> {
  const { data } = await api.get<ApiResponse<UserType[]>>('/user-types');
  return data.data!;
}

export async function getUserTypeById(id: string): Promise<UserType> {
  const { data } = await api.get<ApiResponse<UserType>>(`/user-types/${id}`);
  return data.data!;
}

export async function createUserType(form: UserTypeFormData): Promise<UserType> {
  const { data } = await api.post<ApiResponse<UserType>>('/user-types', {
    name: form.name,
    description: form.description || null,
    sortOrder: form.sortOrder,
    color: form.color || null,
  });
  return data.data!;
}

export async function updateUserType(id: string, form: UserTypeFormData): Promise<UserType> {
  const { data } = await api.put<ApiResponse<UserType>>(`/user-types/${id}`, {
    name: form.name,
    description: form.description || null,
    sortOrder: form.sortOrder,
    color: form.color || null,
  });
  return data.data!;
}

export async function deleteUserType(id: string): Promise<void> {
  await api.delete(`/user-types/${id}`);
}

export async function setUserTypePermissions(id: string, codes: string[]): Promise<UserType> {
  const { data } = await api.put<ApiResponse<UserType>>(`/user-types/${id}/permissions`, {
    permissionCodes: codes,
  });
  return data.data!;
}

export async function setUserTypeMenus(id: string, subMenuIds: string[]): Promise<UserType> {
  const { data } = await api.put<ApiResponse<UserType>>(`/user-types/${id}/menus`, {
    subMenuIds,
  });
  return data.data!;
}

export async function getAllMenus(): Promise<MenuResponse[]> {
  const { data } = await api.get<ApiResponse<MenuResponse[]>>('/menus/all');
  return data.data!;
}

export async function getApiPermissions(group?: string): Promise<ApiPermission[]> {
  const { data } = await api.get<ApiResponse<ApiPermission[]>>('/permissions', {
    params: { group: group || undefined },
  });
  return data.data!;
}
