import axios from 'axios';
import { useAuthStore } from '../store/authStore';

// ── Auth API (login, refresh, OAuth) ──────────────────────────────────────────
export const authApi = axios.create({
  baseURL: '/auth',
  headers: { 'Content-Type': 'application/json' },
});

// ── Backoffice API (all protected resources) ──────────────────────────────────
export const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
});

// Attach Bearer token to every backoffice request
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Auto-refresh on 401
let refreshing = false;
let queue: Array<{ resolve: (t: string) => void; reject: (e: unknown) => void }> = [];

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;
    if (error.response?.status !== 401 || original._retry) {
      return Promise.reject(error);
    }
    original._retry = true;

    if (refreshing) {
      return new Promise((resolve, reject) => {
        queue.push({
          resolve: (token) => {
            original.headers.Authorization = `Bearer ${token}`;
            resolve(api(original));
          },
          reject,
        });
      });
    }

    refreshing = true;
    try {
      const { refreshToken, updateTokens, clearAuth } = useAuthStore.getState();
      if (!refreshToken) throw new Error('No refresh token');

      const { data } = await authApi.post('/refresh', { refreshToken });
      const auth = data.data;
      updateTokens(auth.accessToken, auth.refreshToken, auth.expiresAt);

      queue.forEach((p) => p.resolve(auth.accessToken));
      queue = [];

      original.headers.Authorization = `Bearer ${auth.accessToken}`;
      return api(original);
    } catch (err) {
      queue.forEach((p) => p.reject(err));
      queue = [];
      useAuthStore.getState().clearAuth();
      window.location.href = '/login';
      return Promise.reject(err);
    } finally {
      refreshing = false;
    }
  }
);
