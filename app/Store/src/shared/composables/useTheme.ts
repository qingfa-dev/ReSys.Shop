import { ref, watchEffect } from 'vue'

// Cache: Module-level singleton — shared across all components using useTheme()
const isDark = ref(false)

export function useTheme() {
  function applyTheme(dark: boolean): void {
    isDark.value = dark
    // Cache: Persist theme choice to localStorage for cross-session survival
    document.documentElement.classList.toggle('app-dark', dark)
    localStorage.setItem('resys_theme', dark ? 'dark' : 'light')
  }

  function toggle(): void {
    applyTheme(!isDark.value)
  }

  function init(): void {
    // Cache: Restore theme from localStorage, fallback to OS preference
    const stored = localStorage.getItem('resys_theme')
    if (stored) {
      applyTheme(stored === 'dark')
    } else {
      applyTheme(window.matchMedia('(prefers-color-scheme: dark)').matches)
    }
  }

  // Cache: Keep DOM in sync when isDark changes — covers SSR hydration edge cases
  watchEffect(() => {
    document.documentElement.classList.toggle('app-dark', isDark.value)
  })

  return { isDark, toggle, init }
}
