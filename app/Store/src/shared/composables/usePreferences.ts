import { ref, watch } from 'vue'

const STORAGE_KEY = 'resys-preferences'

interface UserPreferences {
  currency: 'VND' | 'USD' | 'EUR'
  language: 'en' | 'vi'
}

const defaults: UserPreferences = { currency: 'USD', language: 'en' }

function load(): UserPreferences {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? { ...defaults, ...JSON.parse(raw) } : defaults
  } catch { return defaults }
}

const preferences = ref<UserPreferences>(load())

watch(preferences, (val) => {
  try { localStorage.setItem(STORAGE_KEY, JSON.stringify(val)) } catch { /* ignore */ }
}, { deep: true })

export function usePreferences() {
  function formatCurrency(amount: number): string {
    switch (preferences.value.currency) {
      case 'USD': return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount)
      case 'EUR': return new Intl.NumberFormat('de-DE', { style: 'currency', currency: 'EUR' }).format(amount)
      case 'VND': return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
    }
  }

  return { preferences, formatCurrency }
}
