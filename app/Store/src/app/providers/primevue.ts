import type { App } from 'vue'
import Aura from '@primeuix/themes/aura'
import PrimeVue from 'primevue/config'
import ConfirmationService from 'primevue/confirmationservice'
import ToastService from 'primevue/toastservice'
import { env } from '../config/env'

export function registerPrimeVue(app: App): void {
  app.use(PrimeVue, {
    license: env.primeLicenseKey,
    theme: {
      preset: Aura,
      options: {},
    },
  })
  app.use(ToastService)
  app.use(ConfirmationService)
}
