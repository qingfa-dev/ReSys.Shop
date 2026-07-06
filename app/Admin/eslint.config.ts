import { globalIgnores } from 'eslint/config'
import { defineConfigWithVueTs, vueTsConfigs } from '@vue/eslint-config-typescript'
import pluginVue from 'eslint-plugin-vue'
import pluginVitest from '@vitest/eslint-plugin'
import pluginOxlint from 'eslint-plugin-oxlint'
import boundaries from 'eslint-plugin-boundaries'
import skipFormatting from 'eslint-config-prettier/flat'

// To allow more languages other than `ts` in `.vue` files, uncomment the following lines:
// import { configureVueProject } from '@vue/eslint-config-typescript'
// configureVueProject({ scriptLangs: ['ts', 'tsx'] })
// More info at https://github.com/vuejs/eslint-config-typescript/#advanced-setup

export default defineConfigWithVueTs(
  {
    name: 'app/files-to-lint',
    files: ['**/*.{vue,ts,mts,tsx}'],
  },

  globalIgnores(['**/dist/**', '**/dist-ssr/**', '**/coverage/**']),

  ...pluginVue.configs['flat/essential'],
  vueTsConfigs.recommended,

  {
    ...pluginVitest.configs.recommended,
    files: ['src/**/__tests__/*'],
  },

  ...pluginOxlint.buildFromOxlintConfigFile('.oxlintrc.json'),

  {
    plugins: { boundaries },
    settings: {
      'boundaries/elements': [
        { type: 'shared', pattern: 'src/shared/*' },
        { type: 'features', pattern: 'src/features/*', mode: 'folder' },
        { type: 'app', pattern: 'src/app/*', mode: 'folder' },
      ],
    },
    rules: {
      'boundaries/element-types': [
        'error',
        {
          default: 'allow',
          rules: [
            { from: 'shared', disallow: ['features', 'app'] },
            { from: 'features', disallow: ['features', 'app'] },
            { from: 'app', allow: ['shared', 'features'] },
          ],
        },
      ],
      'boundaries/external': [
        'error',
        {
          default: 'disallow',
          rules: [
            {
              from: ['shared', 'features', 'app'],
              allow: [
                'vue',
                'vue-router',
                '@tanstack/vue-query',
                'pinia',
                'primevue',
                'zod',
                'vitest',
                '@vue/test-utils',
                'happy-dom',
                '@testing-library/*',
              ],
            },
          ],
        },
      ],
    },
  },

  skipFormatting,
)
