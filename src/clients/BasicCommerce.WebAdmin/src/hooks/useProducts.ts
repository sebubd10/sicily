import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import * as productsApi from '../api/productsApi';
import type { ProductFormData, ProductListParams } from '../types/product';

const KEY      = ['products'] as const;
const VAT_KEY  = ['vat-rates'] as const;
const CAT_KEY  = ['categories', 'simple-list'] as const;
const MFR_KEY  = ['manufacturers'] as const;
const TAGS_KEY = ['product-tags', 'all'] as const;

export function useProducts(params: ProductListParams) {
  return useQuery({
    queryKey: [...KEY, params],
    queryFn: () => productsApi.getProducts(params),
    placeholderData: keepPreviousData,
  });
}

export function useProductSearch(search: string, enabled = true) {
  return useQuery({
    queryKey: [...KEY, 'search', search],
    queryFn: () => productsApi.getProducts({ search, page: 1, pageSize: 10 }),
    enabled: enabled && search.length >= 2,
    staleTime: 30_000,
    select: (data) => data.items,
  });
}

export function useProductDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => productsApi.getProductById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useCreateProduct() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: ProductFormData) => productsApi.createProduct(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdateProduct() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: string; form: ProductFormData }) =>
      productsApi.updateProduct(id, form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useActivateProduct() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => productsApi.activateProduct(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeactivateProduct() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => productsApi.deactivateProduct(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeleteProduct() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => productsApi.deleteProduct(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

// ── Lookup hooks ─────────────────────────────────────────────────────────────

export function useVatRates() {
  return useQuery({
    queryKey: VAT_KEY,
    queryFn: productsApi.getVatRates,
    staleTime: Infinity,
  });
}

export function useAllCategoriesFlat() {
  return useQuery({
    queryKey: CAT_KEY,
    queryFn: productsApi.getAllCategories,
    staleTime: 60_000,
  });
}

export function useAllManufacturers() {
  return useQuery({
    queryKey: MFR_KEY,
    queryFn: productsApi.getAllManufacturers,
    staleTime: 60_000,
  });
}

export function useAllTags() {
  return useQuery({
    queryKey: TAGS_KEY,
    queryFn: productsApi.getAllTags,
    staleTime: 60_000,
  });
}

// ── Image hooks ───────────────────────────────────────────────────────────────

export function useProductImages(productId: string | null) {
  const qc = useQueryClient();
  const imagesKey = [...KEY, 'images', productId] as const;

  const query = useQuery({
    queryKey: imagesKey,
    queryFn: () => productsApi.getProductImages(productId!),
    enabled: !!productId,
    staleTime: 0,
  });

  const addByUrl = useMutation({
    mutationFn: (args: { title: string; url: string; description?: string }) =>
      productsApi.addImageByUrl(productId!, args.title, args.url, args.description),
    onSuccess: () => qc.invalidateQueries({ queryKey: imagesKey }),
  });

  const addByUpload = useMutation({
    mutationFn: (args: { file: File; title: string; description?: string }) =>
      productsApi.addImageByUpload(productId!, args.file, args.title, args.description),
    onSuccess: () => qc.invalidateQueries({ queryKey: imagesKey }),
  });

  const updateImage = useMutation({
    mutationFn: (args: { imageId: string; title: string; description?: string }) =>
      productsApi.updateProductImage(productId!, args.imageId, args.title, args.description),
    onSuccess: () => qc.invalidateQueries({ queryKey: imagesKey }),
  });

  const deleteImage = useMutation({
    mutationFn: (imageId: string) => productsApi.deleteProductImage(productId!, imageId),
    onSuccess: () => qc.invalidateQueries({ queryKey: imagesKey }),
  });

  const reorder = useMutation({
    mutationFn: (items: { id: string; sortOrder: number }[]) =>
      productsApi.reorderProductImages(productId!, items),
    onSuccess: () => qc.invalidateQueries({ queryKey: imagesKey }),
  });

  return { query, addByUrl, addByUpload, updateImage, deleteImage, reorder };
}
