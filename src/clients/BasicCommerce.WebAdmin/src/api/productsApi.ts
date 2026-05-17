import { api } from './axiosInstance';
import type {
  Product, ProductFormData, ProductListResponse, ProductListParams,
  VatRate, SimpleCategory, SimpleManufacturer, SimpleTag, ProductImage,
} from '../types/product';

type ApiResponse<T> = { success: boolean; data: T; message?: string };

// ── Products ────────────────────────────────────────────────────────────────

export async function getProducts(params: ProductListParams): Promise<ProductListResponse> {
  const { data } = await api.get<ApiResponse<ProductListResponse>>('/products', {
    params: {
      page: params.page,
      pageSize: params.pageSize,
      search: params.search || undefined,
      categoryId: params.categoryId || undefined,
      status: params.includeInactive ? undefined : 'Active',
    },
  });
  const res = data.data!;
  return {
    ...res,
    totalPages: res.totalPages ?? Math.ceil(res.totalCount / res.pageSize),
  };
}

export async function getProductById(id: string): Promise<Product> {
  const { data } = await api.get<ApiResponse<Product>>(`/products/${id}`);
  return data.data!;
}

export async function createProduct(form: ProductFormData): Promise<Product> {
  const { data } = await api.post<ApiResponse<Product>>('/products', toPayload(form, true));
  return data.data!;
}

export async function updateProduct(id: string, form: ProductFormData): Promise<Product> {
  const { data } = await api.put<ApiResponse<Product>>(`/products/${id}`, toPayload(form, false));
  return data.data!;
}

export async function activateProduct(id: string): Promise<void> {
  await api.put(`/products/${id}/activate`);
}

export async function deactivateProduct(id: string): Promise<void> {
  await api.put(`/products/${id}/deactivate`);
}

function toPayload(form: ProductFormData, isCreate: boolean) {
  const base = {
    name: form.name,
    nameBn: form.nameBn,
    description: form.description || null,
    categoryId: form.categoryId,
    price: parseFloat(form.price) || 0,
    vatRateId: form.vatRateId,
    unitType: form.unitType,
    unitLabel: form.unitLabel || null,
    isWeightBased: form.isWeightBased,
    isPerishable: form.isPerishable,
    isAgeRestricted: form.isAgeRestricted,
    ageRestrictionYears: form.isAgeRestricted && form.ageRestrictionYears
      ? parseInt(form.ageRestrictionYears) : null,
    isEbtEligible: form.isEbtEligible,
    trackInventory: form.trackInventory,
    reorderLevel: parseInt(form.reorderLevel) || 0,
    imageUrl: form.imageUrl || null,
    costPrice: form.costPrice ? parseFloat(form.costPrice) : null,
    manufacturerId: form.manufacturerId || null,
    tagIds: form.tagIds,
    plu: form.plu || null,
  };
  if (isCreate) {
    return { ...base, sku: form.sku, barcode: form.barcode };
  }
  return base;
}

// ── Export ───────────────────────────────────────────────────────────────────

export async function exportProductsPdf(params?: {
  search?: string;
  includeInactive?: boolean;
  categoryId?: string;
}): Promise<void> {
  const response = await api.get('/reports/products/pdf', {
    params: {
      search: params?.search || undefined,
      includeInactive: params?.includeInactive ?? false,
      categoryId: params?.categoryId || undefined,
    },
    responseType: 'blob',
  });
  const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
  const a = document.createElement('a');
  a.href = url;
  a.download = `products_${new Date().toISOString().slice(0, 10)}.pdf`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

// ── Lookups ───────────────────────────────────────────────────────────────────

export async function getVatRates(): Promise<VatRate[]> {
  const { data } = await api.get<ApiResponse<VatRate[]>>('/vat-rates');
  return data.data!;
}

export async function getAllCategories(): Promise<SimpleCategory[]> {
  const { data } = await api.get<ApiResponse<{ items: SimpleCategory[] }>>('/categories', {
    params: { pageNumber: 1, pageSize: 500, includeInactive: false },
  });
  return data.data!.items;
}

export async function getAllManufacturers(): Promise<SimpleManufacturer[]> {
  const { data } = await api.get<ApiResponse<SimpleManufacturer[]>>('/manufacturers');
  return data.data!;
}

export async function getAllTags(): Promise<SimpleTag[]> {
  const { data } = await api.get<ApiResponse<{ items: SimpleTag[] }>>('/product-tags', {
    params: { page: 1, pageSize: 500 },
  });
  return data.data!.items;
}

// ── Product Images ────────────────────────────────────────────────────────────

export async function getProductImages(productId: string): Promise<ProductImage[]> {
  const { data } = await api.get<ApiResponse<ProductImage[]>>(`/products/${productId}/images`);
  return data.data!;
}

export async function addImageByUrl(
  productId: string, title: string, url: string,
  description?: string, sortOrder?: number,
): Promise<ProductImage> {
  const { data } = await api.post<ApiResponse<ProductImage>>(
    `/products/${productId}/images/url`,
    { title, url, description: description || null, sortOrder: sortOrder ?? 0 },
  );
  return data.data!;
}

export async function addImageByUpload(
  productId: string, file: File, title: string,
  description?: string, sortOrder?: number,
): Promise<ProductImage> {
  const form = new FormData();
  form.append('file', file);
  form.append('title', title);
  if (description) form.append('description', description);
  form.append('sortOrder', String(sortOrder ?? 0));
  const { data } = await api.post<ApiResponse<ProductImage>>(
    `/products/${productId}/images/upload`, form,
    { headers: { 'Content-Type': 'multipart/form-data' } },
  );
  return data.data!;
}

export async function updateProductImage(
  productId: string, imageId: string, title: string, description?: string,
): Promise<ProductImage> {
  const { data } = await api.put<ApiResponse<ProductImage>>(
    `/products/${productId}/images/${imageId}`,
    { title, description: description || null },
  );
  return data.data!;
}

export async function deleteProductImage(productId: string, imageId: string): Promise<void> {
  await api.delete(`/products/${productId}/images/${imageId}`);
}

export async function reorderProductImages(
  productId: string, items: { id: string; sortOrder: number }[],
): Promise<void> {
  await api.put(`/products/${productId}/images/reorder`, { items });
}
