import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { CUSTOMER_FILTER_FIELDS, CUSTOMER_SORT_FIELDS, CUSTOMER_SEARCH_FIELDS } from '../types/profile'
import type { ProfileListItem } from '../types/profile'

export function useProfileList(options?: UsePagedQueryOptions) {
  return usePagedQuery<ProfileListItem>('api/admin/customer/all', {
    allowedFilterFields: CUSTOMER_FILTER_FIELDS,
    allowedSortFields: CUSTOMER_SORT_FIELDS,
    allowedSearchFields: CUSTOMER_SEARCH_FIELDS,
    ...options,
  })
}
