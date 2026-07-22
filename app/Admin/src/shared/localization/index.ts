import type { App } from 'vue'
import { createI18n } from 'vue-i18n'
import en from './messages/en/general.json'

export function createI18nPlugin() {
  const i18n = createI18n({
    locale: 'en',
    fallbackLocale: 'en',
    messages: { en },
  })

  return {
    install(app: App) {
      app.use(i18n)
    },
  }
}
