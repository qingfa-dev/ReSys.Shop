import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import { installPrimeVue } from './plugins/primevue'
import { installAuthBootstrap } from './plugins/auth-bootstrap'

const app = createApp(App)

app.use(createPinia())

installPrimeVue(app)
installAuthBootstrap(app)

app.use(router)
app.mount('#app')
