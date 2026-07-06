import { useQuery, type UseQueryReturnType } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { api } from '@/shared/api/client'
import type { PagedResult } from '@/shared/api/paged-result'
import type { PageRequest } from '@/shared/types/page'
import type { UserListItem } from '../model/user.types'
import { usersQueryKeys } from './query-keys'

export function useUsersList(
  params: Ref<PageRequest>,
): UseQueryReturnType<PagedResult<UserListItem>, Error> {
  return useQuery({
    queryKey: usersQueryKeys.list(params as unknown as Record<string, unknown>),
    queryFn: () => {
      const search = new URLSearchParams()
      search.set('page', String(params.value.page))
      search.set('pageSize', String(params.value.pageSize))
      if (params.value.search) search.set('search', params.value.search)
      return api.getPaged<UserListItem>(`/api/admin/identity/users?${search.toString()}`)
    },
  })
}
