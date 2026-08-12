import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { RoleApi } from '../services/roleApi'
import type { RoleListItem } from '../types/role'

export function useRoleList(options?: UsePagedQueryOptions) {
  return usePagedQuery<RoleListItem>((params) => RoleApi.getRoles(params), {
    ...options,
  })
}