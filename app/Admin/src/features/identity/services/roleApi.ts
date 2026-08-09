import { post, get, put, patch, del, delWithBody } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult } from '@/shared/types'
import type {
  RoleRequest,
  RoleListItem,
  RoleDetail,
  RoleQuery,
} from '../types/role'
import {
  toRoleQueryParams,
  ROLE_FILTER_FIELDS,
  ROLE_SORT_FIELDS,
  ROLE_SEARCH_FIELDS,
} from '../types/role'
import type { PermissionGroupResponse } from '../types/permission'

export class RoleApi {
  private static readonly BASE = 'api/admin/identity/roles'

  static getRoles(query: RoleQuery): Promise<PagedResult<RoleListItem>> {
    return getPaged<RoleListItem>(RoleApi.BASE, toRoleQueryParams(query), {
      allowedFilterFields: ROLE_FILTER_FIELDS,
      allowedSortFields: ROLE_SORT_FIELDS,
      allowedSearchFields: ROLE_SEARCH_FIELDS,
    })
  }

  static getRole(id: string): Promise<Result<RoleDetail>> {
    return get<Result<RoleDetail>>(`${RoleApi.BASE}/${id}`)
  }

  static createRole(request: RoleRequest): Promise<Result<RoleDetail>> {
    return post<Result<RoleDetail>>(RoleApi.BASE, request)
  }

  static updateRole(id: string, request: RoleRequest): Promise<Result<RoleDetail>> {
    return put<Result<RoleDetail>>(`${RoleApi.BASE}/${id}`, request)
  }

  static deleteRole(id: string): Promise<Result<void>> {
    return del<Result<void>>(`${RoleApi.BASE}/${id}`)
  }

  static assignPermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return put<Result<void>>(`${RoleApi.BASE}/${id}/permissions/assign`, { permissions })
  }

  static getPermissions(id: string): Promise<Result<PermissionGroupResponse>> {
    return get<Result<PermissionGroupResponse>>(`${RoleApi.BASE}/${id}/permissions`)
  }

  static revokePermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return delWithBody<Result<void>>(`${RoleApi.BASE}/${id}/permissions/revoke`, { permissions })
  }

  static syncPermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return patch<Result<void>>(`${RoleApi.BASE}/${id}/permissions/sync`, { permissions })
  }
}
