import { api } from './axiosInstance';
import type { User, UserFormData, UserListParams, PaginatedUsers } from '../types/user';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getUsers(params: UserListParams): Promise<PaginatedUsers> {
  const { data } = await api.get<ApiResponse<PaginatedUsers>>('/users', {
    params: {
      page: params.page,
      pageSize: params.pageSize,
      search: params.search || undefined,
    },
  });
  return data.data!;
}

export async function createUser(form: UserFormData): Promise<User> {
  const { data } = await api.post<ApiResponse<User>>('/users', {
    firstName: form.firstName,
    lastName: form.lastName,
    email: form.email,
    password: form.password,
    role: form.role,
    storeId: form.storeId || null,
    phoneNumber: form.phoneNumber || null,
  });
  return data.data!;
}

export async function activateUser(id: string): Promise<void> {
  await api.put(`/users/${id}/activate`);
}

export async function deactivateUser(id: string): Promise<void> {
  await api.put(`/users/${id}/deactivate`);
}

export async function unlockUser(id: string): Promise<void> {
  await api.put(`/users/${id}/unlock`);
}

export async function assignUserType(id: string, userTypeId: string | null): Promise<void> {
  await api.put(`/users/${id}/user-type`, { userTypeId: userTypeId || null });
}

export async function exportUsersPdf(params: { search?: string }): Promise<void> {
  const response = await api.get('/reports/users/pdf', {
    params: { search: params.search || undefined },
    responseType: 'blob',
  });

  const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
  const a   = document.createElement('a');
  a.href     = url;
  a.download = `users_${new Date().toISOString().slice(0, 10)}.pdf`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}
