import type { App } from 'vue'
import { createI18n } from 'vue-i18n'
import en from './messages/en/general.json'
import auth from './messages/en/auth.json'
import reports from './messages/en/reports.json'

export function createI18nPlugin() {
  const i18n = createI18n({
    locale: 'en',
    fallbackLocale: 'en',
    legacy: false,
    messages: {
      en: {
        ...en,
        auth,
        reports,
      },
    },
  })

  return {
    install(app: App) {
      app.use(i18n)
    },
  }
}
