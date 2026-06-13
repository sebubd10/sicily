export interface User {
  id: string;
  employeeCode: string;
  fullName: string;
  email: string;
  role: string;
  storeName: string | null;
  status: string;
  lastLoginAt: string | null;
  userTypeId: string | null;
  userTypeName: string | null;
  isLocked: boolean;
}

export interface UserFormData {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: string;
  storeId: string;
  phoneNumber: string;
}

export interface UserDetail {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  storeId: string | null;
  storeName: string | null;
  phoneNumber: string | null;
  status: string;
  lastLoginAt: string | null;
  userTypeId: string | null;
  userTypeName: string | null;
  isLocked: boolean;
}

export interface UserEditFormData {
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  storeId: string;
  phoneNumber: string;
}

export interface UserListParams {
  page: number;
  pageSize: number;
  search?: string;
}

export interface PaginatedUsers {
  items: User[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export const USER_ROLES = [
  { value: 'Cashier',      label: 'Cashier' },
  { value: 'Supervisor',   label: 'Supervisor' },
  { value: 'StoreManager', label: 'Store Manager' },
  { value: 'ChainAdmin',   label: 'Chain Admin' },
] as const;
