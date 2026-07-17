import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'
import type { App } from 'vue'

export function installPrimeVue(app: App): void {
  app.use(PrimeVue, {
    theme: { preset: Aura, options: { darkModeSelector: '.app-dark' } },
    ripple: true,
  })
}
