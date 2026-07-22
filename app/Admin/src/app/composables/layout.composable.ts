import { computed, watch } from 'vue'
import { useDarkMode } from '@/shared/composables/useDarkMode'
import { layoutConfig, changeMenuMode as setMenuMode } from './useLayoutConfig'
import { layoutState, hideMobileMenu } from './useLayoutState'

function changeMenuMode(mode: string) {
  setMenuMode(mode, () => {
    layoutState.staticMenuInactive = false
    layoutState.mobileMenuActive = false
    layoutState.sidebarExpanded = false
    layoutState.menuHoverActive = false
  })
}

export function useLayout() {
  const { isDark, toggle } = useDarkMode()

  const isDarkTheme = isDark

  watch(isDarkTheme, (val) => { layoutConfig.darkTheme = val })

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
