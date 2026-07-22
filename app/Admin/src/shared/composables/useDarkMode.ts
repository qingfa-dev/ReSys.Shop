import { ref, watchEffect } from 'vue'

const DARK_MODE_CLASS = 'app-dark'

export function useDarkMode() {
  const stored = localStorage.getItem('resys-admin-dark-mode')
  const isDark = ref(stored === 'true')

  watchEffect(() => {
    localStorage.setItem('resys-admin-dark-mode', String(isDark.value))
    document.documentElement.classList.toggle(DARK_MODE_CLASS, isDark.value)
  })

  function toggle() { isDark.value = !isDark.value }
  function enable() { isDark.value = true }
  function disable() { isDark.value = false }

  return { isDark, toggle, enable, disable }
}
