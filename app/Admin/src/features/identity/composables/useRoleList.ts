import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { ROLE_FILTER_FIELDS, ROLE_SORT_FIELDS, ROLE_SEARCH_FIELDS } from '../types/role'
import type { RoleListItem } from '../types/role'

export function useRoleList(options?: UsePagedQueryOptions) {
  return usePagedQuery<RoleListItem>(`api/admin/identity/roles`, {
    allowedFilterFields: ROLE_FILTER_FIELDS,
    allowedSortFields: ROLE_SORT_FIELDS,
    allowedSearchFields: ROLE_SEARCH_FIELDS,
    ...options,
  })
}
