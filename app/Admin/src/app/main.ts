import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'
import App from './App.vue'
import router from './router'
import { installPrimeVue } from './plugins/primevue'
import { setAuthTokenAccessor } from '@/shared/api/fetch-options'
import { ref } from 'vue'

const tokens = ref<{ accessToken: string } | null>(null)
setAuthTokenAccessor(() => tokens.value?.accessToken ?? null)

const app = createApp(App)

app.use(createPinia())

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 30_000, retry: 1, refetchOnWindowFocus: false },
  },
})
app.use(VueQueryPlugin, { queryClient })

installPrimeVue(app)

const stored = localStorage.getItem('auth:tokens')
if (stored) {
  try {
    tokens.value = JSON.parse(stored) as { accessToken: string }
  } catch {
    localStorage.removeItem('auth:tokens')
  }
}

app.use(router)
app.mount('#app')
