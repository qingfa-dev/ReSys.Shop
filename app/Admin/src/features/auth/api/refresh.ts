import { ref } from 'vue'
import { api } from '@/shared/api/client'
import type { AuthTokens } from '../model/auth.types'

export function useRefresh() {
  const isPending = ref(false)
  const error = ref<Error | null>(null)

  async function mutateAsync(refreshToken: string): Promise<AuthTokens> {
    isPending.value = true
    error.value = null
    try {
      return await api.post<AuthTokens>('/api/auth/refresh', { refreshToken })
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
      throw e
    } finally {
      isPending.value = false
    }
  }

  return { mutateAsync, isPending, error }
}
