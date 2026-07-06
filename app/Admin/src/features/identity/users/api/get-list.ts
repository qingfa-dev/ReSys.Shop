import { ref, watch, type Ref } from 'vue'
import { api } from '@/shared/api/client'
import type { PagedResult } from '@/shared/api/paged-result'
import type { PageRequest } from '@/shared/types/page'
import type { UserListItem } from '../model/user.types'

export function useUsersList(params: Ref<PageRequest>) {
  const data = ref<PagedResult<UserListItem> | null>(null)
  const isLoading = ref(false)
  const error = ref<Error | null>(null)

  async function load(): Promise<void> {
    isLoading.value = true
    error.value = null
    try {
      const search = new URLSearchParams()
      search.set('page', String(params.value.page))
      search.set('pageSize', String(params.value.pageSize))
      if (params.value.search) search.set('search', params.value.search)
      data.value = await api.getPaged<UserListItem>(`/api/admin/identity/users?${search.toString()}`)
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
      data.value = null
    } finally {
      isLoading.value = false
    }
  }

  watch(params, load, { immediate: true })

  return { data, isLoading, error, refetch: load }
}
