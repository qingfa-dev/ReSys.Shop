import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { IDENTITY } from '@/shared/constants/api'
import { USER_FILTER_FIELDS, USER_SORT_FIELDS, USER_SEARCH_FIELDS } from '../types/user'
import type { UserListItem } from '../types/user'

export function useUserList(options?: UsePagedQueryOptions) {
  return usePagedQuery<UserListItem>(`${IDENTITY}/users`, {
    allowedFilterFields: USER_FILTER_FIELDS,
    allowedSortFields: USER_SORT_FIELDS,
    allowedSearchFields: USER_SEARCH_FIELDS,
    ...options,
  })
}
