import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// The dev server proxies /api -> the .NET backend so the browser makes same-origin calls
// (avoids CORS in development). Override the target with VITE_API_TARGET if needed.
export default defineConfig(({ mode }) => {
  const target = process.env.VITE_API_TARGET || 'http://localhost:5000'
  return {
    plugins: [react()],
    server: {
      port: 5173,
      proxy: {
        '/api': {
          target,
          changeOrigin: true,
          rewrite: (p) => p.replace(/^\/api/, ''),
        },
      },
    },
  }
})
