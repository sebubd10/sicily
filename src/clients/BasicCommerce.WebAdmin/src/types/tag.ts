export type TagStatus = 'Active' | 'Inactive';

export type TagDetail = {
  id: string;
  name: string;
  taggedProductsCount: number;
  status: TagStatus;
  createdAt: string;
  updatedAt: string | null;
};

export type TagListResponse = {
  items: TagDetail[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export type TagListParams = {
  page: number;
  pageSize: number;
  search?: string;
};

export type TagFormData = {
  name: string;
};
