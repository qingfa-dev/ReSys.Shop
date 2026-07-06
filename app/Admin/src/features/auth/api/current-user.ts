import { ref } from 'vue'
import { api } from '@/shared/api/client'
import type { AuthUser } from '../model/auth.types'

export function useCurrentUser() {
  const data = ref<AuthUser | null>(null)
  const isLoading = ref(false)
  const error = ref<Error | null>(null)

  async function load(): Promise<void> {
    isLoading.value = true
    error.value = null
    try {
      data.value = await api.get<AuthUser>('/api/auth/me')
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
      data.value = null
    } finally {
      isLoading.value = false
    }
  }

  load()

  return { data, isLoading, error, refetch: load }
}
