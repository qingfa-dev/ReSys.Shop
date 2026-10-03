import { reactive } from 'vue'

export const layoutState = reactive({
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

export function hideMobileMenu() {
  layoutState.mobileMenuActive = false
}
