import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { setupPrimeVue } from '@/app/plugins/primevue'
import { createI18nPlugin } from '@/shared/localization'
import { createDirectivesPlugin } from '@/shared/directives'

import './assets/styles/tailwind.css'
import './assets/styles/main.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)
setupPrimeVue(app)
app.use(createI18nPlugin())
app.use(createDirectivesPlugin())

app.mount('#app')
