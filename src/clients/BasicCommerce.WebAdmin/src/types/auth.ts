export interface UserDto {
  id: string;
  fullName: string;
  email: string;
  role: string;
  tenantId: string;
  storeId?: string;
  userTypeId?: string;
  authProvider: string;
  preferredLanguage: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  tokenType: string;
  user: UserDto;
}

export interface LoginRequest {
  email: string;
  password: string;
  tenantSlug?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}
