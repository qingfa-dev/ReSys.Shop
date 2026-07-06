import { ref, computed } from 'vue'
import { useQueryClient } from '@tanstack/vue-query'
import { setAuthTokenAccessor } from '@/shared/api/fetch-options'
import { api } from '@/shared/api/client'
import { authQueryKeys } from '../api/query-keys'
import { useLogin } from '../api/login'
import { useLogout } from '../api/logout'
import { useCurrentUser } from '../api/current-user'
import type { AuthTokens, AuthUser } from '../model/auth.types'

const tokens = ref<AuthTokens | null>(null)

setAuthTokenAccessor(() => tokens.value?.accessToken ?? null)

export function useAuthState() {
  const qc = useQueryClient()
  const currentUser = useCurrentUser()
  const login = useLogin()
  const logout = useLogout()

  const isAuthenticated = computed(() => !!tokens.value)

  async function setTokens(t: AuthTokens) {
    tokens.value = t
    const user = await api.get<AuthUser>('/api/auth/me')
    qc.setQueryData(authQueryKeys.currentUser(), user)
  }

  function clear() {
    tokens.value = null
    qc.removeQueries({ queryKey: authQueryKeys.all })
  }

  return { tokens, isAuthenticated, user: currentUser, login, logout, setTokens, clear }
}
