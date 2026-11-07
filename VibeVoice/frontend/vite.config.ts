import { fileURLToPath, URL } from 'node:url'
import basicSsl from '@vitejs/plugin-basic-ssl'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

export default defineConfig(({ mode }) => ({
  // `npm run dev:lan` (mode "lan") serves HTTPS with a self-signed certificate: browsers only allow the
  // microphone and clipboard on secure pages, and http://192.168.x.x is not one (localhost is).
  plugins: [react(), tailwindcss(), mode === 'lan' && basicSsl({ name: 'vibevoice-dev' })],
  resolve: {
    alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
  },
  server: {
    port: 5173,
    strictPort: true,
    // Same-origin /api in development: no CORS, and LAN devices reach the API through Vite while the
    // API itself keeps listening on localhost only. Override the target with VITE_API_PROXY.
    proxy: {
      '/api': { target: process.env.VITE_API_PROXY ?? 'http://localhost:5080', changeOrigin: true },
    },
  },
  preview: { port: 5173 },
  build: {
    rollupOptions: {
      output: {
        manualChunks(id: string) {
          if (!id.includes('node_modules')) return undefined
          if (id.includes('@tiptap') || id.includes('prosemirror')) return 'editor'
          if (id.includes('wavesurfer')) return 'wavesurfer'
          if (id.includes('date-fns')) return 'date'
          if (/[\\/]node_modules[\\/](react|react-dom|scheduler|react-router)[\\/]/.test(id)) return 'react'
          if (id.includes('i18next')) return 'i18n'
          if (id.includes('zod') || id.includes('react-hook-form') || id.includes('@hookform')) return 'forms'
          if (id.includes('@tanstack')) return 'query'
          return 'vendor'
        },
      },
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    // Only the palette sheet is processed (read as ?raw by the contrast test); other CSS is stubbed.
    css: { include: [/palettes\.css/] },
    restoreMocks: true,
  },
}))
