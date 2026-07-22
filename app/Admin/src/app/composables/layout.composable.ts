import { computed, reactive, watch } from 'vue'
import { useDarkMode } from '@/shared/composables/useDarkMode'

const STORAGE_KEY = 'resys-admin-layout'

interface LayoutConfig {
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

const layoutConfig = reactive<LayoutConfig>({
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
  const { isDark, toggle } = useDarkMode()

  const isDarkTheme = isDark

  watch(isDarkTheme, (val) => { layoutConfig.darkTheme = val })
  watch(() => layoutConfig.darkTheme, (val) => { if (val !== isDarkTheme.value) isDarkTheme.value = val })

  function toggleDarkMode() {
    if (!document.startViewTransition) {
      toggle()
      return
    }
    const transition = document.startViewTransition(() => toggle())
    transition.ready.then(() => {
      const x = window.innerWidth / 2
      const y = window.innerHeight / 2
      const endRadius = Math.hypot(window.innerWidth, window.innerHeight)
      document.documentElement.animate(
        { clipPath: [`circle(0 at ${x}px ${y}px)`, `circle(${endRadius}px at ${x}px ${y}px)`] },
        { duration: 400, easing: 'ease-in', pseudoElement: '::view-transition-new(root)' },
      )
    })
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
