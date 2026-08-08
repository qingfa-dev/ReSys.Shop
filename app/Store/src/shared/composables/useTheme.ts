import { ref, watchEffect } from 'vue'

const isDark = ref(false)

export function useTheme() {
  function applyTheme(dark: boolean): void {
    isDark.value = dark
    document.documentElement.classList.toggle('dark', dark)
    localStorage.setItem('resys_theme', dark ? 'dark' : 'light')
  }

  function toggle(): void {
    applyTheme(!isDark.value)
  }

  function init(): void {
    const stored = localStorage.getItem('resys_theme')
    if (stored) {
      applyTheme(stored === 'dark')
    } else {
      applyTheme(window.matchMedia('(prefers-color-scheme: dark)').matches)
    }
  }

  watchEffect(() => {
    document.documentElement.classList.toggle('dark', isDark.value)
  })

  return { isDark, toggle, init }
}
