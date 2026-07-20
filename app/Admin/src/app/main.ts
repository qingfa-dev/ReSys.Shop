import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import StyleClass from 'primevue/styleclass'
import Aura from '@primeuix/themes/aura'

import App from './App.vue'
import router from './router'
import { useLayout } from '@/app/layout/composables/layout.composable'

import '@/assets/tailwind.css'
import '@/assets/styles.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: '.app-dark',
    },
  },
  ripple: true,
})
app.use(ToastService)
app.use(ConfirmationService)
app.directive('styleclass', StyleClass)

const { layoutConfig } = useLayout()
if (layoutConfig.darkTheme) {
  document.documentElement.classList.add('app-dark')
}

app.mount('#app')
