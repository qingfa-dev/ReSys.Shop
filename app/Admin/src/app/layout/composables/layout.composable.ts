import { computed, ref } from 'vue'

const preset = ref('Aura')
const primary = ref('emerald')
const surface = ref<string | null>(null)
const darkTheme = ref(false)
const menuMode = ref('static')
const staticMenuInactive = ref(false)
const overlayMenuActive = ref(false)
const profileSidebarVisible = ref(false)
const configSidebarVisible = ref(false)
const sidebarExpanded = ref(false)
const menuHoverActive = ref(false)
const activeMenuItem = ref<string | null>(null)
const activePath = ref<string | null>(null)
const mobileMenuActive = ref(false)
const anchored = ref(false)

export function useLayout() {
  const layoutConfig = {
    get preset() { return preset.value },
    set preset(v: string) { preset.value = v },
    get primary() { return primary.value },
    set primary(v: string) { primary.value = v },
    get surface() { return surface.value },
    set surface(v: string | null) { surface.value = v },
    get darkTheme() { return darkTheme.value },
    set darkTheme(v: boolean) { darkTheme.value = v },
    get menuMode() { return menuMode.value },
    set menuMode(v: string) { menuMode.value = v },
  }

  const layoutState = {
    get staticMenuInactive() { return staticMenuInactive.value },
    set staticMenuInactive(v: boolean) { staticMenuInactive.value = v },
    get overlayMenuActive() { return overlayMenuActive.value },
    set overlayMenuActive(v: boolean) { overlayMenuActive.value = v },
    get profileSidebarVisible() { return profileSidebarVisible.value },
    set profileSidebarVisible(v: boolean) { profileSidebarVisible.value = v },
    get configSidebarVisible() { return configSidebarVisible.value },
    set configSidebarVisible(v: boolean) { configSidebarVisible.value = v },
    get sidebarExpanded() { return sidebarExpanded.value },
    set sidebarExpanded(v: boolean) { sidebarExpanded.value = v },
    get menuHoverActive() { return menuHoverActive.value },
    set menuHoverActive(v: boolean) { menuHoverActive.value = v },
    get activeMenuItem() { return activeMenuItem.value },
    set activeMenuItem(v: string | null) { activeMenuItem.value = v },
    get activePath() { return activePath.value },
    set activePath(v: string | null) { activePath.value = v },
    get mobileMenuActive() { return mobileMenuActive.value },
    set mobileMenuActive(v: boolean) { mobileMenuActive.value = v },
    get anchored() { return anchored.value },
    set anchored(v: boolean) { anchored.value = v },
  }

  const isDarkTheme = computed(() => darkTheme.value)

  function toggleDarkMode() {
    if (!document.startViewTransition) {
      darkTheme.value = !darkTheme.value
      document.documentElement.classList.toggle('app-dark', darkTheme.value)
      return
    }
    document.startViewTransition(() => {
      darkTheme.value = !darkTheme.value
      document.documentElement.classList.toggle('app-dark', darkTheme.value)
    })
  }

  const isDesktop = () => window.innerWidth > 991

  function toggleMenu() {
    if (isDesktop()) {
      if (menuMode.value === 'static') {
        staticMenuInactive.value = !staticMenuInactive.value
      }
      if (menuMode.value === 'overlay') {
        overlayMenuActive.value = !overlayMenuActive.value
      }
    } else {
      mobileMenuActive.value = !mobileMenuActive.value
    }
  }

  function toggleConfigSidebar() {
    configSidebarVisible.value = !configSidebarVisible.value
  }

  function hideMobileMenu() {
    mobileMenuActive.value = false
  }

  function changeMenuMode(mode: string) {
    menuMode.value = mode
    staticMenuInactive.value = false
    mobileMenuActive.value = false
    sidebarExpanded.value = false
    menuHoverActive.value = false
    anchored.value = false
  }

  const hasOpenOverlay = computed(() => overlayMenuActive.value)

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
