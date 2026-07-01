import { fileURLToPath, URL } from 'node:url'

import tailwind from '@tailwindcss/vite'
import ui from '@nuxt/ui/vite'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'

export default defineConfig({
  plugins: [
    tailwind(),
    vue(),
    vueJsx(),
    ui({
      ui: {
        colors: {
          primary: 'amber',
          neutral: 'zinc',
        },
      },
    }),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
})
