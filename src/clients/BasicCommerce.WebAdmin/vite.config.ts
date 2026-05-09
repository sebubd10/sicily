import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      // Only proxy AJAX calls to the Auth API — NOT /auth/callback (that is a React route)
      '/auth/login':     { target: 'http://localhost:5000', changeOrigin: true, secure: false },
      '/auth/refresh':   { target: 'http://localhost:5000', changeOrigin: true, secure: false },

      // Backoffice API — all protected resource calls
      '/api': { target: 'http://localhost:5001', changeOrigin: true, secure: false },
    },
  },
});
