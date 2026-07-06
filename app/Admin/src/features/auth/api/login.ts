import { ref } from 'vue'
import { api } from '@/shared/api/client'
import type { LoginRequest, AuthTokens } from '../model/auth.types'

export function useLogin() {
  const isPending = ref(false)
  const error = ref<Error | null>(null)

  async function mutateAsync(body: LoginRequest): Promise<AuthTokens> {
    isPending.value = true
    error.value = null
    try {
      return await api.post<AuthTokens>('/api/auth/login', body)
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
      throw e
    } finally {
      isPending.value = false
    }
  }

  return { mutateAsync, isPending, error }
}
