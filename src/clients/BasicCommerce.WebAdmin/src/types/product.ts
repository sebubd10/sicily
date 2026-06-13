export type ProductStatus = 'Active' | 'Inactive';

export type ProductTag = {
  id: string;
  name: string;
};

export type ProductImage = {
  id: string;
  productId: string;
  title: string;
  description: string | null;
  url: string;
  isUploaded: boolean;
  sortOrder: number;
  createdAt: string;
  updatedAt: string | null;
};

export type Product = {
  id: string;
  sku: string;
  barcode: string;
  plu: string | null;
  name: string;
  nameBn: string;
  description: string | null;
  categoryId: string;
  categoryName: string;
  categoryStatus: 'Active' | 'Inactive' | 'Deleted';
  price: number;
  currency: string;
  costPrice: number | null;
  vatRateId: string;
  vatRateName: string;
  vatRate: number;
  unitType: string;
  unitLabel: string | null;
  isWeightBased: boolean;
  isPerishable: boolean;
  isAgeRestricted: boolean;
  ageRestrictionYears: number | null;
  isEbtEligible: boolean;
  trackInventory: boolean;
  reorderLevel: number;
  status: ProductStatus;
  imageUrl: string | null;
  manufacturerId: string | null;
  manufacturerName: string | null;
  tags: ProductTag[];
  createdAt: string;
  updatedAt: string | null;
};

export type ProductFormData = {
  sku: string;
  barcode: string;
  plu: string;
  name: string;
  nameBn: string;
  description: string;
  categoryId: string;
  manufacturerId: string;
  price: string;
  costPrice: string;
  vatRateId: string;
  unitType: string;
  unitLabel: string;
  isWeightBased: boolean;
  isPerishable: boolean;
  isAgeRestricted: boolean;
  ageRestrictionYears: string;
  isEbtEligible: boolean;
  trackInventory: boolean;
  reorderLevel: string;
  imageUrl: string;
  tagIds: string[];
};

export type ProductListItem = {
  id: string;
  sku: string;
  name: string;
  nameBn: string;
  categoryName: string;
  price: number;
  currency: string;
  vatRate: number;
  imageUrl: string | null;
  manufacturerName: string | null;
  status: ProductStatus;
};

export type ProductListResponse = {
  items: ProductListItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export type ProductListParams = {
  page: number;
  pageSize: number;
  search?: string;
  categoryId?: string;
  includeInactive?: boolean;
};

export type VatRate = {
  id: string;
  name: string;
  code: string;
  rate: number;
  isDefault: boolean;
  status: string;
};

export type SimpleCategory = {
  id: string;
  name: string;
  parentCategoryId: string | null;
  status: 'Active' | 'Inactive';
};

export type SimpleManufacturer = {
  id: string;
  name: string;
};

export type SimpleTag = {
  id: string;
  name: string;
  status: 'Active' | 'Inactive';
};

export const UNIT_TYPES = ['Each', 'Weight', 'Volume', 'Length', 'Pack'] as const;
