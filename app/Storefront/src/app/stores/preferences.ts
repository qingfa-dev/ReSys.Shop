import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'

export type Theme = 'light' | 'dark' | 'system'

export interface UserPreferences {
  theme: 'light' | 'dark' | 'system'
  currency: string
  language: string
  notifications: {
    email: boolean
    sms: boolean
    push: boolean
  }
  newsletter: boolean
}

const STORAGE_KEY = 'userPreferences'

const defaultPreferences: UserPreferences = {
  theme: 'system',
  currency: 'USD',
  language: 'en',
  notifications: {
    email: true,
    sms: false,
    push: false,
  },
  newsletter: true,
}

function loadFromStorage(): UserPreferences {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored) {
      return { ...defaultPreferences, ...JSON.parse(stored) }
    }
  } catch (e) {
    console.error('Failed to load preferences:', e)
  }
  return defaultPreferences
}

export const usePreferencesStore = defineStore('preferences', () => {
  const preferences = ref<UserPreferences>(loadFromStorage())

  watch(
    preferences,
    (newValue) => {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(newValue))
      applyTheme(newValue.theme)
    },
    { deep: true }
  )

  function applyTheme(theme: 'light' | 'dark' | 'system') {
    const isDark =
      theme === 'dark' ||
      (theme === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches)

    if (isDark) {
      document.documentElement.classList.add('dark')
    } else {
      document.documentElement.classList.remove('dark')
    }
  }

  function setTheme(theme: 'light' | 'dark' | 'system') {
    preferences.value.theme = theme
  }

  function toggleTheme() {
    const current = preferences.value.theme
    if (current === 'dark') {
      preferences.value.theme = 'light'
    } else if (current === 'light') {
      preferences.value.theme = 'dark'
    } else {
      preferences.value.theme = 'dark'
    }
  }

  const isDark = computed(() => {
    const theme = preferences.value.theme
    if (theme === 'system') {
      return window.matchMedia('(prefers-color-scheme: dark)').matches
    }
    return theme === 'dark'
  })

  function setCurrency(currency: string) {
    preferences.value.currency = currency
  }

  function setLanguage(language: string) {
    preferences.value.language = language
  }

  function setNotifications(key: keyof UserPreferences['notifications'], value: boolean) {
    preferences.value.notifications[key] = value
  }

  function setNewsletter(value: boolean) {
    preferences.value.newsletter = value
  }

  function reset() {
    preferences.value = { ...defaultPreferences }
  }

  applyTheme(preferences.value.theme)

  if (typeof window !== 'undefined') {
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
      if (preferences.value.theme === 'system') {
        applyTheme('system')
      }
    })
  }

  return {
    preferences,
    theme: computed(() => preferences.value.theme),
    isDark,
    applyTheme,
    setTheme,
    toggleTheme,
    setCurrency,
    setLanguage,
    setNotifications,
    setNewsletter,
    reset,
  }
})