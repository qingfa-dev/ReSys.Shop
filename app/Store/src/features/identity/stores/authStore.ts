import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AuthUser } from '../types/auth'
import * as authApi from '../services/authApi'
import * as tokenService from '../services/tokenService'
import { setTokenGetter } from '@/shared/api/interceptors/auth'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthUser | null>(null)
  const status = ref<'idle' | 'loading' | 'authenticated' | 'error'>('idle')
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => status.value === 'authenticated' && user.value !== null)

  // Init: Validate token and hydrate session (called once by router guard)
  async function init(): Promise<void> {
    if (!tokenService.hasValidAccessToken()) {
      status.value = 'idle'
      return
    }
    try {
      const result = await authApi.getSession()
      if (result.isSuccess) {
        user.value = {
          userId: result.value.id,
          userName: result.value.userName,
          email: result.value.email,
          roles: result.value.roles,
          permissions: result.value.permissions,
          isAuthenticated: true,
        }
        status.value = 'authenticated'
      } else {
        tokenService.clearTokens()
        status.value = 'idle'
      }
    } catch {
      tokenService.clearTokens()
      status.value = 'idle'
    }
  }

  // Login: Authenticate with password
  async function login(credential: string, password: string): Promise<boolean> {
    status.value = 'loading'
    error.value = null
    const result = await authApi.login({ credential, password })
    if (result.isSuccess) {
      tokenService.setTokens(result.value)
      setTokenGetter(tokenService.getAccessToken)
      const sessionResult = await authApi.getSession()
      if (sessionResult.isSuccess) {
        user.value = {
          userId: sessionResult.value.id,
          userName: sessionResult.value.userName,
          email: sessionResult.value.email,
          roles: sessionResult.value.roles,
          permissions: sessionResult.value.permissions,
          isAuthenticated: true,
        }
        status.value = 'authenticated'
        return true
      }
    }
    status.value = 'error'
    error.value = result.message ?? result.errors[0]?.message ?? 'Login failed'
    return false
  }

  // Login: Redirect to Google OAuth
  async function loginWithGoogle(): Promise<void> {
    const result = await authApi.getLoginProviders()
    if (result.isSuccess) {
      const provider = result.value.find(p => p.name.toLowerCase() === 'google')
      if (provider) {
        window.location.href = provider.url
      }
    }
  }

  // Logout: Revoke tokens and clear state
  async function logout(revokeAll?: boolean): Promise<void> {
    try { await authApi.logout({ revokeAll }) } catch { /* fire-and-forget */ }
    tokenService.clearTokens()
    user.value = null
    status.value = 'idle'
    error.value = null
  }

  return { user, status, error, isAuthenticated, init, login, loginWithGoogle, logout }
})
