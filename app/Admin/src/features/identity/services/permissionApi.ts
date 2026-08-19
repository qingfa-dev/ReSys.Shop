import { getPaged } from '@/shared/api'

import type { PagedResult } from '@/shared/types'
import type { PermissionMetadata } from '../types/permission'

export class PermissionApi {
  static getPermissions(): Promise<PagedResult<PermissionMetadata>> {
    return getPaged<PermissionMetadata>('/api/admin/identity/permissions', { pageNumber: 1, pageSize: 100 })
  }
}