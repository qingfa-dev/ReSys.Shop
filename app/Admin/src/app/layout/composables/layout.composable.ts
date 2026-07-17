import { ref } from 'vue'

const isDarkMode = ref(false)
const sidebarActive = ref(false)

export function useLayout() {
  function toggleDarkMode() {
    isDarkMode.value = !isDarkMode.value
    document.documentElement.classList.toggle('app-dark', isDarkMode.value)
  }

  function toggleSidebar() {
    sidebarActive.value = !sidebarActive.value
  }

  return { isDarkMode, toggleDarkMode, sidebarActive, toggleSidebar }
}
