import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import type { App } from 'vue'

export function installPrimeVue(app: App): void {
  app.use(PrimeVue, {
    theme: {
      preset: Aura,
      options: { darkModeSelector: '.p-dark' },
    },
    ripple: true,
  })
  app.use(ToastService)
  app.use(ConfirmationService)
}
