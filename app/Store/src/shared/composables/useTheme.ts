import { ref, computed, onUnmounted } from 'vue'

export type ThemeMode = 'light' | 'dark' | 'system'

const STORAGE_KEY = 'theme-preference'
const DARK_CLASS = 'app-dark'

let mediaQuery: MediaQueryList | null = null
let mediaListener: ((e: MediaQueryListEvent) => void) | null = null

const currentMode = ref<ThemeMode>(readStoredMode())

function readStoredMode(): ThemeMode {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored === 'light' || stored === 'dark' || stored === 'system') return stored
  } catch { /* localStorage unavailable */ }
  return 'system'
}

function systemPrefersDark(): boolean {
  if (typeof window === 'undefined') return false
  return window.matchMedia('(prefers-color-scheme: dark)').matches
}

function applyClass(dark: boolean): void {
  if (typeof document === 'undefined') return
  document.documentElement.classList.toggle(DARK_CLASS, dark)
}

function persist(mode: ThemeMode): void {
  try { localStorage.setItem(STORAGE_KEY, mode) } catch { /* ignore */ }
}

function startListening(): void {
  if (typeof window === 'undefined') return
  mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
  mediaListener = () => {
    if (currentMode.value === 'system') {
      applyClass(systemPrefersDark())
    }
  }
  mediaQuery.addEventListener('change', mediaListener)
}

function stopListening(): void {
  if (mediaQuery && mediaListener) {
    mediaQuery.removeEventListener('change', mediaListener)
    mediaQuery = null
    mediaListener = null
  }
}

export function useTheme() {
  const isDark = computed(() => {
    if (currentMode.value === 'dark') return true
    if (currentMode.value === 'light') return false
    return systemPrefersDark()
  })

  function setMode(mode: ThemeMode): void {
    currentMode.value = mode
    persist(mode)
    applyClass(isDark.value)
  }

  function toggle(): void {
    const order: ThemeMode[] = ['light', 'dark', 'system']
    const idx = order.indexOf(currentMode.value)
    setMode(order[(idx + 1) % order.length] ?? 'light')
  }

  if (typeof document !== 'undefined') {
    applyClass(isDark.value)
    startListening()
  }

  onUnmounted(() => {
    stopListening()
  })

  return { mode: currentMode, isDark, toggle, setMode }
}
