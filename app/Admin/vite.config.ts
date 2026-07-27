import { fileURLToPath, URL } from 'node:url'

import tailwind from '@tailwindcss/vite'
import { PrimeVueResolver } from '@primevue/auto-import-resolver'
import Components from 'unplugin-vue-components/vite'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
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
      '@layouts': fileURLToPath(new URL('./src/app/layouts', import.meta.url)),
      '@providers': fileURLToPath(new URL('./src/app/providers', import.meta.url)),
      '@router': fileURLToPath(new URL('./src/app/router', import.meta.url)),
      '@ui': fileURLToPath(new URL('./src/shared/components/ui', import.meta.url)),
      '@feedback': fileURLToPath(new URL('./src/shared/components/feedback', import.meta.url)),
      '@forms': fileURLToPath(new URL('./src/shared/components/forms', import.meta.url)),
      '@tables': fileURLToPath(new URL('./src/shared/components/tables', import.meta.url)),
      '@navigation': fileURLToPath(new URL('./src/shared/components/navigation', import.meta.url)),
      '@config': fileURLToPath(new URL('./src/app/config', import.meta.url)),
    },
  },
})
