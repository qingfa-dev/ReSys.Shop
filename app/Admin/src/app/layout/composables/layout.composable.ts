import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'

export interface MenuItem {
  label: string
  icon?: string
  to?: string
  items?: MenuItem[]
  visible?: boolean
}

export function useLayout() {
  const router = useRouter()

  const STORAGE_KEY_SIDEBAR = 'admin:sidebar:collapsed'
  const collapsed = ref(localStorage.getItem(STORAGE_KEY_SIDEBAR) === '1')
  const isDark = ref(false)

  const darkMode = computed(() => isDark.value)
  const sidebarCollapsed = computed(() => collapsed.value)

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
    isDark.value = !isDark.value
    document.documentElement.classList.toggle('p-dark', isDark.value)
  }

  function toggleSidebar() {
    collapsed.value = !collapsed.value
    localStorage.setItem(STORAGE_KEY_SIDEBAR, collapsed.value ? '1' : '0')
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
