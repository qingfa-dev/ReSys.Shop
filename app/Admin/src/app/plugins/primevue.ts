import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import type { App } from 'vue'

export function installPrimeVue(app: App): void {
  app.use(PrimeVue, {
    theme: {
      preset: Aura,
      options: {
        darkModeSelector: '.app-dark',
        transitionDuration: '0.2s',
      },
    },
    ripple: true,
  })
  app.use(ToastService)
  app.use(ConfirmationService)
}
