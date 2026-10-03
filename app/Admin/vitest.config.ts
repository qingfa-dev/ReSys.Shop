import { fileURLToPath } from 'node:url'
import { mergeConfig, defineConfig, configDefaults } from 'vitest/config'
import viteConfig from './vite.config'

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      exclude: [...configDefaults.exclude, 'e2e/**'],
      root: fileURLToPath(new URL('./', import.meta.url)),
      coverage: {
        provider: 'v8',
        include: ['src/shared/types/**', 'src/shared/utils/**', 'src/shared/constants/**', 'src/shared/api/**', 'src/shared/composables/**'],
        exclude: ['src/shared/**/index.ts', 'src/shared/**/*.spec.ts', 'src/shared/composables/useLayout.ts'],
        thresholds: {
          statements: 65,
          branches: 65,
          functions: 65,
          lines: 65,
        },
      },
    },
  }),
)
