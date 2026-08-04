import { readFileSync } from 'node:fs'
import { fileURLToPath, URL } from 'node:url'

import { PrimeVueResolver } from '@primevue/auto-import-resolver'
import tailwind from '@tailwindcss/vite'
import Components from 'unplugin-vue-components/vite'
import AutoImport from 'unplugin-auto-import/vite'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'

const pkg = JSON.parse(readFileSync(fileURLToPath(new URL('./package.json', import.meta.url)), 'utf-8'))

const aspireApiUrl = process.env.services__api__https__0 || process.env.services__api__http__0

export default defineConfig({
  define: {
    __APP_VERSION__: JSON.stringify(pkg.version),
    ...(aspireApiUrl ? { 'import.meta.env.VITE_API_URL': JSON.stringify(aspireApiUrl) } : {}),
  },
  plugins: [
    tailwind(),
    vue(),
    vueJsx(),
    vueDevTools(),
    Components({ resolvers: [PrimeVueResolver()] }),
    AutoImport({
      imports: ['vue', 'vue-router'],
      dirs: ['src/common/composables'],
      dts: 'src/auto-imports.d.ts',
      eslintrc: { enabled: true },
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
      '/api': { target: aspireApiUrl || process.env.VITE_API_URL || 'http://localhost:5035', changeOrigin: true, secure: false },
    },
  },
})
