import { readFileSync } from 'node:fs'
import { fileURLToPath, URL } from 'node:url'

const pkg = JSON.parse(readFileSync(new URL('./package.json', import.meta.url), 'utf-8'))

import tailwind from '@tailwindcss/vite'
import { PrimeVueResolver } from '@primevue/auto-import-resolver'
import Components from 'unplugin-vue-components/vite'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  define: {
    __APP_VERSION__: JSON.stringify(pkg.version),
  },
  plugins: [
    vue(),
    vueJsx(),
    vueDevTools(),
    tailwind(),
    Components({
      resolvers: [PrimeVueResolver()],
    }),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      '@providers': fileURLToPath(new URL('./src/app/providers', import.meta.url)),
      '@router': fileURLToPath(new URL('./src/app/router', import.meta.url)),
      '@layout': fileURLToPath(new URL('./src/shared/components/layout', import.meta.url)),
      '@panel': fileURLToPath(new URL('./src/shared/components/panel', import.meta.url)),
      '@data': fileURLToPath(new URL('./src/shared/components/data', import.meta.url)),
      '@overlay': fileURLToPath(new URL('./src/shared/components/overlay', import.meta.url)),
      '@form': fileURLToPath(new URL('./src/shared/components/form', import.meta.url)),
      '@config': fileURLToPath(new URL('./src/app/config', import.meta.url)),
    },
  },
})
