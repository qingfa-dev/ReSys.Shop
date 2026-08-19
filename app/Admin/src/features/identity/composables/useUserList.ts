import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { UserApi } from '../services/userApi'
import type { UserListItem } from '../types/user'

export function useUserList(options?: UsePagedQueryOptions) {
  return usePagedQuery<UserListItem>((params) => UserApi.getUsers(params), {
    ...options,
  })
}