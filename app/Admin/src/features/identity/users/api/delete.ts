import { ref } from 'vue'
import { api } from '@/shared/api/client'
import type { UserId } from '@/shared/types/id'

export function useDeleteUser() {
  const isPending = ref(false)
  const error = ref<Error | null>(null)

  async function mutateAsync(id: UserId): Promise<void> {
    isPending.value = true
    error.value = null
    try {
      await api.delete<void>(`/api/admin/identity/users/${id}`)
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
      throw e
    } finally {
      isPending.value = false
    }
  }

  return { mutateAsync, isPending, error }
}
