// Matches CategoryResponse from the backend (camelCase JSON)
export type Category = {
  id: string;
  name: string;
  nameBn: string;
  description: string | null;
  parentCategoryId: string | null;
  parentCategoryName: string | null;
  sortOrder: number;
  status: 'Active' | 'Inactive';
  childCount: number;
};

export type CategoryFormData = {
  name: string;
  nameBn: string;
  description: string;
  parentCategoryId: string;
  sortOrder: number;
};

export type PaginatedCategories = {
  items: Category[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export type CategoryListParams = {
  page: number;
  pageSize: number;
  search?: string;
  includeInactive?: boolean;
};
