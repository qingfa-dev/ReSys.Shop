import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ui from '@nuxt/ui/vue-plugin'

import App from './App.vue'
import router from './router'
import './assets/css/main.css'
import './assets/shop/main.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(ui)

app.mount('#app')
