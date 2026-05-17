import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { UserDto } from '../types/auth';

interface AuthState {
  user: UserDto | null;
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: string | null;
  isAuthenticated: boolean;

  setAuth: (accessToken: string, refreshToken: string, expiresAt: string, user: UserDto) => void;
  updateTokens: (accessToken: string, refreshToken: string, expiresAt: string) => void;
  clearAuth: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      expiresAt: null,
      isAuthenticated: false,

      setAuth: (accessToken, refreshToken, expiresAt, user) =>
        set({ accessToken, refreshToken, expiresAt, user, isAuthenticated: true }),

      updateTokens: (accessToken, refreshToken, expiresAt) =>
        set({ accessToken, refreshToken, expiresAt }),

      clearAuth: () =>
        set({ user: null, accessToken: null, refreshToken: null, expiresAt: null, isAuthenticated: false }),
    }),
    { name: 'bc-auth' }
  )
);
