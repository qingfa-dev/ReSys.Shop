import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'
import Components from 'unplugin-vue-components/vite'
import { PrimeVueResolver } from '@primevue/auto-import-resolver'

export default defineConfig({
  plugins: [
    vue(),
    tailwindcss(),
    Components({
      resolvers: [PrimeVueResolver()],
      dts: true,
    }),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  test: {
    environment: 'jsdom',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      include: [
        'src/core/**/*.ts',
        'src/features/**/*.mock.repository.ts',
        'src/features/**/__tests__/**/*.ts',
        'src/app/composables/**/*.ts',
        'src/app/stores/**/*.ts',
      ],
      exclude: [
        'src/**/*.spec.ts',
        'src/main.ts',
        'src/**/*.d.ts',
        'src/env.d.ts',
        'src/components.d.ts',
        'src/**/README.md',
        'src/**/*.md',
        'src/**/index.ts',
        'src/core/services/**',
        'src/**/data/**',
      ],
      thresholds: {
        perFile: true,
        lines: 90,
        functions: 100,
        branches: 70,
        statements: 90,
      },
    },
  },
})
