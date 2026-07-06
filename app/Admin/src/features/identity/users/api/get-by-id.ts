import { ref, watch, type Ref } from 'vue'
import { api } from '@/shared/api/client'
import type { User } from '../model/user.types'

export function useUser(id: Ref<string | null>) {
  const data = ref<User | null>(null)
  const isLoading = ref(false)
  const error = ref<Error | null>(null)

  async function load(): Promise<void> {
    if (!id.value) {
      data.value = null
      return
    }
    isLoading.value = true
    error.value = null
    try {
      data.value = await api.get<User>(`/api/admin/identity/users/${id.value}`)
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
      data.value = null
    } finally {
      isLoading.value = false
    }
  }

  watch(id, load, { immediate: true })

  return { data, isLoading, error, refetch: load }
}
