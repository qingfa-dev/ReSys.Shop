import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'
import App from './App.vue'
import router from './router'
import { installPrimeVue } from './plugins/primevue'
import { installAuthBootstrap } from './plugins/auth-bootstrap'

const app = createApp(App)

app.use(createPinia())

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 30_000, retry: 1, refetchOnWindowFocus: false },
  },
})
app.use(VueQueryPlugin, { queryClient })

installPrimeVue(app)
installAuthBootstrap(app)

app.use(router)
app.mount('#app')
