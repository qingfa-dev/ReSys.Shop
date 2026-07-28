import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import Lara from '@primeuix/themes/aura'

import App from './App.vue'
import router from './app/router'
import { usePreferencesStore } from './app/stores/preferences'
import '@/assets/main.scss'
import '@/assets/css/main.css'
import '@/assets/shop/main.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: Lara,
    options: {
      darkModeSelector: '.dark',
    },
  },
})
app.use(ToastService)

usePreferencesStore()

app.mount('#app')
