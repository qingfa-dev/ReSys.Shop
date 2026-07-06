import { ref } from 'vue'
import { api } from '@/shared/api/client'
import type { User, UserCreateRequest } from '../model/user.types'

export function useCreateUser() {
  const isPending = ref(false)
  const error = ref<Error | null>(null)

  async function mutateAsync(body: UserCreateRequest): Promise<User> {
    isPending.value = true
    error.value = null
    try {
      return await api.post<User>('/api/admin/identity/users', body)
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
      throw e
    } finally {
      isPending.value = false
    }
  }

  return { mutateAsync, isPending, error }
}
