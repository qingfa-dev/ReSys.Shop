import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import StyleClass from 'primevue/styleclass'
import Aura from '@primeuix/themes/aura'
import App from './App.vue'
import router from './router'
import { installAuthBootstrap } from './auth/auth-bootstrap'
import 'primeicons/primeicons.css'
import '@/assets/tailwind.css'
import '@/assets/scss/main.scss'

const app = createApp(App)
app.use(createPinia())
app.use(PrimeVue, { theme: { preset: Aura, options: { darkModeSelector: '.app-dark' } }, ripple: true })
app.use(ToastService)
app.use(ConfirmationService)
app.directive('styleclass', StyleClass)
installAuthBootstrap(app)
app.use(router)
app.mount('#app')
