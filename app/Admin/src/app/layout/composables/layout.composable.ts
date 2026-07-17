import { computed, reactive } from 'vue'

const layoutConfig = reactive({
  preset: 'Aura',
  primary: 'emerald',
  surface: null as string | null,
  darkTheme: false,
  menuMode: 'static',
})

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
