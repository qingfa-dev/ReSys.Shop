import { ref, computed } from 'vue'
import { setAuthTokenAccessor } from '@/shared/api/fetch-options'
import { api } from '@/shared/api/client'
import { useLogin } from '../api/login'
import { useLogout } from '../api/logout'
import { useCurrentUser } from '../api/current-user'
import type { AuthTokens, AuthUser } from '../model/auth.types'

const tokens = ref<AuthTokens | null>(null)

setAuthTokenAccessor(() => tokens.value?.accessToken ?? null)

export function useAuthState() {
  const currentUser = useCurrentUser()
  const login = useLogin()
  const logout = useLogout()

  const isAuthenticated = computed(() => !!tokens.value)

  async function setTokens(t: AuthTokens) {
    tokens.value = t
    localStorage.setItem('auth:tokens', JSON.stringify(t))
    const user = await api.get<AuthUser>('/api/auth/me')
    currentUser.data.value = user
  }

  function clear() {
    tokens.value = null
    localStorage.removeItem('auth:tokens')
    currentUser.data.value = null
  }

  return { tokens, isAuthenticated, user: currentUser, login, logout, setTokens, clear }
}
