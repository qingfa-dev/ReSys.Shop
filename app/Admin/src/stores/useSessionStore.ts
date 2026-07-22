import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

interface CurrentUser {
  id: string
  email: string
  name: string
  role: string
  permissions: string[]
}

export const useSessionStore = defineStore('session', () => {
  const user = ref<CurrentUser | null>(null)
  const isLoading = ref(true)

  const isAuthenticated = computed(() => user.value !== null)

  function setUser(newUser: CurrentUser) {
    user.value = newUser
    isLoading.value = false
  }

  function clear() {
    user.value = null
    isLoading.value = false
  }

  return { user, isAuthenticated, isLoading, setUser, clear }
})
