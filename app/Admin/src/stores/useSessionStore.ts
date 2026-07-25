import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { CurrentUser } from '@/shared/types/user'

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

  return { user: readonly(user), isAuthenticated, isLoading: readonly(isLoading), setUser, clear }
})
