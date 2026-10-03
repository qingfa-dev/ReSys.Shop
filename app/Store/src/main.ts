import { createApp } from 'vue'
import App from './App.vue'
import router from './app/router'
import { registerPrimeVue } from '@providers/primevue'
import { registerPinia } from '@providers/pinia'

import '@/assets/main.css'

const app = createApp(App)

app.use(router)
registerPinia(app)
registerPrimeVue(app)

app.mount('#app')
