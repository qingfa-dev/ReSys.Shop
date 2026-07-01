// --- vite.config.ts ---
// Vite build configuration for the Admin SPA.
// Plugin order: @tailwindcss/vite first (processes @import "tailwindcss"),
// then Vue + Vue JSX, then PrimeVue auto-import resolver.
// The `@` alias maps to ./src for clean imports.
// ---

import { fileURLToPath, URL } from 'node:url'

import { PrimeVueResolver } from '@primevue/auto-import-resolver'
import tailwind from '@tailwindcss/vite'
import Components from 'unplugin-vue-components/vite'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'

export default defineConfig({
  plugins: [
    tailwind(),
    vue(),
    vueJsx(),
    vueDevTools(),
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
    port: 5173,
    proxy: {
      '/api': { target: process.env.VITE_API_URL || 'http://localhost:5035', changeOrigin: true },
    },
  },
})
