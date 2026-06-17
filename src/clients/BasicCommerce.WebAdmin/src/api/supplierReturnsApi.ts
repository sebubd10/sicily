import { api } from './axiosInstance';
import type {
  SupplierReturnDetail,
  SupplierReturnListResponse,
  CreateSupplierReturnFormData,
  AddSupplierReturnItemFormData,
} from '../types/supplierReturn';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

export async function getSupplierReturns(params: {
  page?: number;
  pageSize?: number;
  status?: string;
  supplierId?: string;
  storeId?: string;
}): Promise<SupplierReturnListResponse> {
  const { data } = await api.get<ApiResponse<SupplierReturnListResponse>>('/supplier-returns', {
    params: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
      status: params.status || undefined,
      supplierId: params.supplierId || undefined,
      storeId: params.storeId || undefined,
    },
  });
  return data.data!;
}

export async function getSupplierReturnById(id: string): Promise<SupplierReturnDetail> {
  const { data } = await api.get<ApiResponse<SupplierReturnDetail>>(`/supplier-returns/${id}`);
  return data.data!;
}

export async function createSupplierReturn(
  form: CreateSupplierReturnFormData,
): Promise<SupplierReturnDetail> {
  const { data } = await api.post<ApiResponse<SupplierReturnDetail>>('/supplier-returns', {
    supplierId: form.supplierId,
    storeId: form.storeId,
    purchaseOrderId: form.purchaseOrderId || null,
    notes: form.notes || null,
  });
  return data.data!;
}

export async function addSupplierReturnItem(
  id: string,
  item: AddSupplierReturnItemFormData,
): Promise<SupplierReturnDetail> {
  const { data } = await api.post<ApiResponse<SupplierReturnDetail>>(
    `/supplier-returns/${id}/items`,
    {
      productId: item.productId,
      quantity: item.quantity,
      unitCost: item.unitCost,
      reason: item.reason,
      notes: item.notes || null,
    },
  );
  return data.data!;
}

export async function removeSupplierReturnItem(
  id: string,
  productId: string,
): Promise<SupplierReturnDetail> {
  const { data } = await api.delete<ApiResponse<SupplierReturnDetail>>(
    `/supplier-returns/${id}/items/${productId}`,
  );
  return data.data!;
}

export async function setExpectedCredit(
  id: string,
  expectedCreditAmount: number,
): Promise<SupplierReturnDetail> {
  const { data } = await api.put<ApiResponse<SupplierReturnDetail>>(
    `/supplier-returns/${id}/expected-credit`,
    { expectedCreditAmount },
  );
  return data.data!;
}

export async function submitSupplierReturn(id: string): Promise<SupplierReturnDetail> {
  const { data } = await api.post<ApiResponse<SupplierReturnDetail>>(
    `/supplier-returns/${id}/submit`,
  );
  return data.data!;
}

export async function shipSupplierReturn(id: string): Promise<SupplierReturnDetail> {
  const { data } = await api.post<ApiResponse<SupplierReturnDetail>>(
    `/supplier-returns/${id}/ship`,
  );
  return data.data!;
}

export async function receiveCredit(
  id: string,
  creditAmount: number,
  creditNoteReference?: string,
): Promise<SupplierReturnDetail> {
  const { data } = await api.post<ApiResponse<SupplierReturnDetail>>(
    `/supplier-returns/${id}/receive-credit`,
    { creditAmount, creditNoteReference: creditNoteReference || null },
  );
  return data.data!;
}

export async function cancelSupplierReturn(id: string): Promise<SupplierReturnDetail> {
  const { data } = await api.post<ApiResponse<SupplierReturnDetail>>(
    `/supplier-returns/${id}/cancel`,
  );
  return data.data!;
}
