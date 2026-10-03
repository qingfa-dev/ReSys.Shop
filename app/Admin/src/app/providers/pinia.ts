import type { App } from 'vue'
import { createPinia } from 'pinia'

export function registerPinia(app: App): void {
  app.use(createPinia())
}
