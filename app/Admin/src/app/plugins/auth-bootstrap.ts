import type { App } from 'vue'
import { useAuthState } from '@/features/auth/composables/useAuthState'
import type { AuthTokens } from '@/features/auth/model/auth.types'

export function installAuthBootstrap(app: App): void {
  const stored = localStorage.getItem('auth:tokens')
  if (!stored) return
  try {
    const tokens = JSON.parse(stored) as AuthTokens
    useAuthState().setTokens(tokens)
  } catch {
    localStorage.removeItem('auth:tokens')
  }
  void app
}
