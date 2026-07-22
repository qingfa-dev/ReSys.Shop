import { reactive, watch } from 'vue'

const STORAGE_KEY = 'resys-admin-layout'

export interface LayoutConfig {
  preset: string
  primary: string
  surface: string | null
  darkTheme: boolean
  menuMode: string
}

function loadConfig(): Partial<LayoutConfig> {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : {}
  } catch {
    return {}
  }
}

const saved = loadConfig()

export const layoutConfig = reactive<LayoutConfig>({
  preset: saved.preset || 'Aura',
  primary: saved.primary || 'emerald',
  surface: (saved.surface as string | null) || null,
  darkTheme: saved.darkTheme ?? false,
  menuMode: saved.menuMode || 'static',
})

watch(
  () => ({ ...layoutConfig }),
  (val) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(val))
  },
  { deep: true },
)

export function changeMenuMode(mode: string) {
  layoutConfig.menuMode = mode
}
