import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// During development the SPA runs on port 5173 and proxies API calls to the
// ASP.NET Core backend on port 5080, so the app uses same-origin "/api" URLs
// in both development and production (where the API serves the built SPA).
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5080',
        changeOrigin: true,
      },
    },
  },
  build: {
    // The production build is emitted here and copied into the API's wwwroot at publish time.
    outDir: 'dist',
  },
})
