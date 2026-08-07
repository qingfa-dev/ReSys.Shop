import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AuthUser } from '../types/auth'
import * as authApi from '../services/authApi'
import * as emailApi from '../services/emailApi'
import * as tokenService from '../services/tokenService'
import { setTokenGetter } from '@/shared/api/interceptors/auth'
import { useCartStore } from '@/features/ordering/stores/cartStore'

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
    try {
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
          // Cart merge: Associate the guest cart with the authenticated user and
          // hydrate the merged cart. Best-effort — a cart-merge failure must not
          // block the authenticated session.
          try {
            const cart = useCartStore()
            await cart.associate()
            await cart.fetchCart()
          } catch {
            /* fire-and-forget: cart merge is non-critical to auth */
          }
          return true
        }
        tokenService.clearTokens()
        status.value = 'error'
        error.value = sessionResult.message ?? sessionResult.errors[0]?.message ?? 'Failed to load session'
        return false
      }
      status.value = 'error'
      error.value = result.message ?? result.errors[0]?.message ?? 'Login failed'
      return false
    } catch {
      // The axios client rejects on network failures / non-Result 5xx. Never
      // leave the UI stranded in 'loading': clear any persisted tokens, surface
      // the error, and resolve false so the caller can stop spinning.
      tokenService.clearTokens()
      status.value = 'error'
      error.value = 'Unable to sign in. Please try again.'
      return false
    }
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

  // Email: Request email change
  async function changeEmail(newEmail: string): Promise<boolean> {
    error.value = null
    try {
      const result = await emailApi.changeEmail(newEmail)
      if (result.isSuccess) return true
      error.value = result.message ?? 'Failed to request email change'
      return false
    } catch {
      error.value = 'Failed to request email change'
      return false
    }
  }

  // Email: Confirm email with token
  async function confirmEmail(token: string): Promise<boolean> {
    error.value = null
    try {
      const result = await emailApi.confirmEmail(token)
      if (result.isSuccess) return true
      error.value = result.message ?? 'Failed to confirm email'
      return false
    } catch {
      error.value = 'Failed to confirm email'
      return false
    }
  }

  // Email: Resend verification email
  async function resendVerification(): Promise<boolean> {
    error.value = null
    try {
      const result = await emailApi.resendVerification()
      if (result.isSuccess) return true
      error.value = result.message ?? 'Failed to resend verification'
      return false
    } catch {
      error.value = 'Failed to resend verification'
      return false
    }
  }

  return { user, status, error, isAuthenticated, init, login, loginWithGoogle, logout, changeEmail, confirmEmail, resendVerification }
})
