import { getPaged } from '@/shared/api'
import { IDENTITY } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types'
import type { PermissionMetadata } from '../types/permission'

export class PermissionApi {
  private static readonly BASE = `${IDENTITY}/permissions`

  static getPermissions(): Promise<PagedResult<PermissionMetadata>> {
    return getPaged<PermissionMetadata>(PermissionApi.BASE, { pageNumber: 1, pageSize: 100 })
  }
}
