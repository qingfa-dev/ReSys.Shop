import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { PROFILE } from '@/shared/constants/api'
import { PROFILE_FILTER_FIELDS, PROFILE_SORT_FIELDS, PROFILE_SEARCH_FIELDS } from '../types/profile'
import type { ProfileListItem } from '../types/profile'

export function useProfileList(options?: UsePagedQueryOptions) {
  return usePagedQuery<ProfileListItem>(`${PROFILE}/profiles/all`, {
    allowedFilterFields: PROFILE_FILTER_FIELDS,
    allowedSortFields: PROFILE_SORT_FIELDS,
    allowedSearchFields: PROFILE_SEARCH_FIELDS,
    ...options,
  })
}
