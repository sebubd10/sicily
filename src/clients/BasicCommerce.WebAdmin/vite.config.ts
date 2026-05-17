import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      // Auth endpoints (login, refresh) — AJAX calls only
      // /auth/google and /auth/microsoft are navigated directly (full URL via env var)
      // /auth/callback is a React route — must NOT be proxied
      '/auth/login':   { target: 'http://localhost:5001', changeOrigin: true, secure: false },
      '/auth/refresh': { target: 'http://localhost:5001', changeOrigin: true, secure: false },

      // All protected resource API calls
      '/api': { target: 'http://localhost:5001', changeOrigin: true, secure: false },
    },
  },
});
