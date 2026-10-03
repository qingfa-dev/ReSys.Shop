import { computed } from 'vue'
import { useAuthStore } from '@/features/identity/store/auth'

export function useAuth() {
  const store = useAuthStore()

  const isLoggedIn = computed(() => store.isAuthenticated)
  const user = computed(() => store.user)
  const isLoading = computed(() => store.loading)
  const error = computed(() => store.error)

  async function initialize() {
    await store.initialize()
  }

  async function login(email: string, password: string, rememberMe = false) {
    await store.login({ email, password, rememberMe })
  }

  async function register(email: string, password: string, firstName: string, lastName: string, phone?: string) {
    await store.register({ email, password, firstName, lastName, phone })
  }

  async function logout() {
    await store.logout()
  }

  async function refreshProfile() {
    await store.fetchProfile()
  }

  return {
    isLoggedIn,
    user,
    isLoading,
    error,
    initialize,
    login,
    register,
    logout,
    refreshProfile,
  }
}
