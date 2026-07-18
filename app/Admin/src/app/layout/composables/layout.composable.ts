import { computed, reactive, watch } from 'vue'

const STORAGE_KEY = 'resys-admin-layout'

function loadConfig(): Partial<typeof layoutConfig> {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : {}
  } catch {
    return {}
  }
}

const saved = loadConfig()

const layoutConfig = reactive({
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
  { deep: true }
)

const layoutState = reactive({
  staticMenuInactive: false,
  overlayMenuActive: false,
  profileSidebarVisible: false,
  configSidebarVisible: false,
  sidebarExpanded: false,
  menuHoverActive: false,
  activeMenuItem: null as string | null,
  activePath: null as string | null,
  mobileMenuActive: false,
})

export function useLayout() {
  const isDarkTheme = computed(() => layoutConfig.darkTheme)

  const executeDarkModeToggle = () => {
    layoutConfig.darkTheme = !layoutConfig.darkTheme
    document.documentElement.classList.toggle('app-dark')
  }

  function toggleDarkMode() {
    if (!document.startViewTransition) {
      executeDarkModeToggle()
      return
    }
    document.startViewTransition(() => executeDarkModeToggle())
  }

  const isDesktop = () => window.innerWidth > 991

  function toggleMenu() {
    if (isDesktop()) {
      if (layoutConfig.menuMode === 'static') {
        layoutState.staticMenuInactive = !layoutState.staticMenuInactive
      }
      if (layoutConfig.menuMode === 'overlay') {
        layoutState.overlayMenuActive = !layoutState.overlayMenuActive
      }
    } else {
      layoutState.mobileMenuActive = !layoutState.mobileMenuActive
    }
  }

  function toggleConfigSidebar() {
    layoutState.configSidebarVisible = !layoutState.configSidebarVisible
  }

  function hideMobileMenu() {
    layoutState.mobileMenuActive = false
  }

  function changeMenuMode(mode: string) {
    layoutConfig.menuMode = mode
    layoutState.staticMenuInactive = false
    layoutState.mobileMenuActive = false
    layoutState.sidebarExpanded = false
    layoutState.menuHoverActive = false
  }

  const hasOpenOverlay = computed(() => layoutState.overlayMenuActive || layoutState.mobileMenuActive)

  return {
    layoutConfig,
    layoutState,
    isDarkTheme,
    toggleDarkMode,
    toggleConfigSidebar,
    toggleMenu,
    hideMobileMenu,
    changeMenuMode,
    isDesktop,
    hasOpenOverlay,
  }
}
