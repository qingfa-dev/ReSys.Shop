import { ref } from 'vue'
import { api } from '@/shared/api/client'
import type { User, UserUpdateRequest } from '../model/user.types'

export function useUpdateUser() {
  const isPending = ref(false)
  const error = ref<Error | null>(null)

  async function mutateAsync(body: UserUpdateRequest): Promise<User> {
    isPending.value = true
    error.value = null
    try {
      return await api.put<User>(`/api/admin/identity/users/${body.id}`, body)
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
      throw e
    } finally {
      isPending.value = false
    }
  }

  return { mutateAsync, isPending, error }
}
