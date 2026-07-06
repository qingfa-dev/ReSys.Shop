import { ref } from 'vue'
import { api } from '@/shared/api/client'

export function useLogout() {
  const isPending = ref(false)
  const error = ref<Error | null>(null)

  async function mutateAsync(): Promise<void> {
    isPending.value = true
    error.value = null
    try {
      await api.post<void>('/api/auth/logout')
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
      throw e
    } finally {
      isPending.value = false
    }
  }

  return { mutateAsync, isPending, error }
}
