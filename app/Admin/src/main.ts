// --- src/main.ts ---
// Application entry point. Bootstraps Vue 3 with Pinia, Vue Router,
// PrimeVue v4 (Aura preset via @primevue/themes), Tailwind v4 CSS,
// and custom Sakai-derived layout styles (sekai).
// Dark mode is controlled via the `.p-dark` class on :root.
// ---

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'

import App from './App.vue'
import router from './router'
import './assets/main.css'
import './assets/sekai/main.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: '.p-dark',
    },
  },
})

app.mount('#app')
