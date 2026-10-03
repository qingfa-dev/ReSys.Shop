import { fileURLToPath, URL } from 'node:url'
import { defineConfig, loadEnv, type UserConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import tailwindcss from '@tailwindcss/vite'
import Components from 'unplugin-vue-components/vite'
import { PrimeVueResolver } from '@primevue/auto-import-resolver'

export default defineConfig(({ mode }): UserConfig => {
  const env = loadEnv(mode, process.cwd(), '')

  const gatewayUrl = env.VITE_GATEWAY_URL ||
    env.GATEWAY_HTTPS ||
    env.GATEWAY_HTTP ||
    'https://localhost:5000';

  const apiPrefix = env.VITE_API_PREFIX || 'resys-api';

  return {
    plugins: [
      vue(),
      vueDevTools(),
      tailwindcss(),
      Components({
        resolvers: [PrimeVueResolver()],
        dts: true,
      }),
    ],

    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
        '@tokens': fileURLToPath(new URL('./src/styles/tokens', import.meta.url)),
        '@components': fileURLToPath(new URL('./src/components', import.meta.url)),
      },
    },

    build: {
      cssMinify: 'esbuild',
    },

    server: {
      port: parseInt(env.PORT ?? '5174'),
      proxy: {
        '/api': {
          target: gatewayUrl,
          changeOrigin: true,
          secure: false,
          rewrite: (path) => path.replace(/^\/api/, `/${apiPrefix}`),
        },
      },
    }
  }
})
