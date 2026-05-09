export interface UserType {
  id: string;
  name: string;
  description: string | null;
  isSystem: boolean;
  sortOrder: number;
  color: string | null;
  allowedSubMenuIds: string[] | null;
  permissionCodes: string[] | null;
}

export interface UserTypeFormData {
  name: string;
  description: string;
  sortOrder: number;
  color: string;
}

export interface SubMenuResponse {
  id: string;
  name: string;
  icon: string | null;
  route: string;
  permissionCode: string | null;
  sortOrder: number;
}

export interface MenuResponse {
  id: string;
  name: string;
  icon: string | null;
  sortOrder: number;
  items: SubMenuResponse[];
}

export interface ApiPermission {
  id: string;
  code: string;
  name: string;
  group: string;
  description: string | null;
}
