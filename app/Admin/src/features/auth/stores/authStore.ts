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
  const isLoggingOut = ref(false)

  const isAuthenticated = computed(() => status.value === 'authenticated' && user.value !== null)

  const currentUser = computed(() => user.value)

  function hasRole(role: string): boolean {
    return user.value?.roles.includes(role) ?? false
  }

  function hasPermission(perm: string): boolean {
    return user.value?.permissions.includes(perm) ?? false
  }

  async function login(credential: string, password: string): Promise<void> {
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
      } else {
        status.value = 'error'
        error.value = 'Failed to fetch session'
      }
    } else {
      status.value = 'error'
      error.value = result.message ?? result.errors[0]?.message ?? 'Login failed'
    }
  }

  async function logout(revokeAll?: boolean): Promise<void> {
    isLoggingOut.value = true
    try {
      await authApi.logout({ revokeAll })
    } catch {
      // Fire-and-forget — always clear local state
    }

    tokenService.clearTokens()
    user.value = null
    status.value = 'idle'
    error.value = null
    isLoggingOut.value = false
  }

  async function init(): Promise<void> {
    if (!tokenService.hasValidAccessToken()) {
      status.value = 'idle'
      return
    }

    try {
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
      } else {
        tokenService.clearTokens()
        status.value = 'idle'
      }
    } catch {
      tokenService.clearTokens()
      status.value = 'idle'
    }
  }

  async function fetchSession(): Promise<void> {
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
    }
  }

  return {
    user,
    status,
    error,
    isLoggingOut,
    isAuthenticated,
    currentUser,
    hasRole,
    hasPermission,
    login,
    logout,
    init,
    fetchSession,
  }
})
