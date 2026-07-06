import { ref, computed } from 'vue'
import { useThemeStore } from '@/app/stores/theme.store'
import { useSidebarStore } from '@/app/stores/sidebar.store'
import { useRouter } from 'vue-router'

export interface MenuItem {
  label: string
  icon?: string
  to?: string
  items?: MenuItem[]
  visible?: boolean
}

export function useLayout() {
  const theme = useThemeStore()
  const sidebar = useSidebarStore()
  const router = useRouter()

  const darkMode = computed(() => theme.isDark)
  const sidebarCollapsed = computed(() => sidebar.collapsed)

  const menuItems = ref<MenuItem[]>([
    {
      label: 'Dashboard',
      icon: 'pi pi-home',
      to: '/',
    },
    {
      label: 'Identity',
      icon: 'pi pi-users',
      items: [
        { label: 'Users', icon: 'pi pi-user', to: '/identity/users' },
      ],
    },
    {
      label: 'Catalog',
      icon: 'pi pi-book',
      items: [
        { label: 'Products', icon: 'pi pi-box', to: '/catalog/products' },
      ],
    },
  ])

  function toggleDarkMode() {
    theme.toggle()
  }

  function toggleSidebar() {
    sidebar.toggle()
  }

  function navigate(to: string) {
    router.push(to)
  }

  return {
    darkMode,
    sidebarCollapsed,
    menuItems,
    toggleDarkMode,
    toggleSidebar,
    navigate,
  }
}
