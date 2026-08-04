import type { App } from 'vue'
import PrimeVue from 'primevue/config'
import ConfirmationService from 'primevue/confirmationservice'
import ToastService from 'primevue/toastservice'
import Tooltip from 'primevue/tooltip'
import { AdminPreset } from '@/assets/presets/admin-preset'

export function setupPrimeVue(app: App) {
  app.use(PrimeVue, {
    license: import.meta.env.VITE_PRIMEVUE_LICENSE_KEY,
    theme: {
      preset: AdminPreset,
      options: {
        darkModeSelector: '.app-dark',
        cssLayer: {
          name: 'primevue',
          order: 'base, primevue, utilities',
        },
      },
    },
  })
  app.use(ConfirmationService)
  app.use(ToastService)
  app.directive('tooltip', Tooltip)
}
