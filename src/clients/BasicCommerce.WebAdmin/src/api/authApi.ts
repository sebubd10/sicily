import { authApi } from './axiosInstance';
import type { AuthResponse, ApiResponse, LoginRequest } from '../types/auth';

export async function login(payload: LoginRequest): Promise<AuthResponse> {
  const { data } = await authApi.post<ApiResponse<AuthResponse>>('/login', payload);
  return data.data;
}

export async function refreshTokens(refreshToken: string): Promise<AuthResponse> {
  const { data } = await authApi.post<ApiResponse<AuthResponse>>('/refresh', { refreshToken });
  return data.data;
}

export function getGoogleOAuthUrl(tenantSlug?: string): string {
  const slug = tenantSlug ? `?tenantSlug=${encodeURIComponent(tenantSlug)}` : '';
  return `/auth/google${slug}`;
}

export function getMicrosoftOAuthUrl(tenantSlug?: string): string {
  const slug = tenantSlug ? `?tenantSlug=${encodeURIComponent(tenantSlug)}` : '';
  return `/auth/microsoft${slug}`;
}
