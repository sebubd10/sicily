import { api } from './axiosInstance';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export interface Customer {
  id: string;
  code: string;
  name: string;
  email: string | null;
  phone: string | null;
  loyaltyPoints: number;
  status: string;
}

export async function searchCustomers(term: string, page = 1, pageSize = 20): Promise<Customer[]> {
  const { data } = await api.get<ApiResponse<Customer[]>>('/customers', {
    params: { term: term || undefined, page, pageSize },
  });
  return data.data!;
}

export async function getCustomerById(id: string): Promise<Customer> {
  const { data } = await api.get<ApiResponse<Customer>>(`/customers/${id}`);
  return data.data!;
}
