import { fileURLToPath, URL } from 'node:url'

import tailwind from '@tailwindcss/vite'
import Components from 'unplugin-vue-components/vite'
import { PrimeVueResolver } from '@primevue/auto-import-resolver'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'

const aspireApiUrl = process.env.services__api__https__0 || process.env.services__api__http__0

export default defineConfig({
  build: {
    cssMinify: 'esbuild',
  },
  define: aspireApiUrl ? {
    'import.meta.env.VITE_API_URL': JSON.stringify(aspireApiUrl)
  } : {},
  plugins: [
    tailwind(),
    vue(),
    vueJsx(),
    Components({
      resolvers: [PrimeVueResolver()],
    }),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5174,
    proxy: {
      '/api': { target: aspireApiUrl || process.env.VITE_API_URL || 'http://localhost:5035', changeOrigin: true, secure: false },
    },
  },
})
