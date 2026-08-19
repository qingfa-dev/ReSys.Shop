import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { ProfileApi } from '../services/profileApi'
import type { ProfileListItem } from '../types/profile'

export function useProfileList(options?: UsePagedQueryOptions) {
  return usePagedQuery<ProfileListItem>((params) => ProfileApi.getProfiles(params), {
    ...options,
  })
}