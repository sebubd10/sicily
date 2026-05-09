import { authApi } from './axiosInstance';
import type { AuthResponse, ApiResponse, LoginRequest } from '../types/auth';

// Backend base URL — browser navigates here directly for OAuth (bypasses Vite proxy)
const AUTH_API_URL = import.meta.env.VITE_AUTH_API_URL ?? 'http://localhost:5000';

export async function login(payload: LoginRequest): Promise<AuthResponse> {
  const { data } = await authApi.post<ApiResponse<AuthResponse>>('/login', payload);
  return data.data;
}

export async function refreshTokens(refreshToken: string): Promise<AuthResponse> {
  const { data } = await authApi.post<ApiResponse<AuthResponse>>('/refresh', { refreshToken });
  return data.data;
}

// Full URL so the browser navigates directly to the backend — OAuth redirect
// chains don't work through the Vite proxy.
export function getGoogleOAuthUrl(tenantSlug?: string): string {
  const qs = tenantSlug ? `?tenantSlug=${encodeURIComponent(tenantSlug)}` : '';
  return `${AUTH_API_URL}/auth/google${qs}`;
}

export function getMicrosoftOAuthUrl(tenantSlug?: string): string {
  const qs = tenantSlug ? `?tenantSlug=${encodeURIComponent(tenantSlug)}` : '';
  return `${AUTH_API_URL}/auth/microsoft${qs}`;
}
