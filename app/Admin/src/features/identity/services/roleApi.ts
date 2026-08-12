import { post, get, put, patch, del, delWithBody } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  RoleRequest,
  RoleListItem,
  RoleDetail,
} from '../types/role'
import {
  ROLE_FILTER_FIELDS,
  ROLE_SORT_FIELDS,
  ROLE_SEARCH_FIELDS,
} from '../types/role'
import type { PermissionGroupResponse } from '../types/permission'

export class RoleApi {
  static getRoles(params: QueryingParameters): Promise<PagedResult<RoleListItem>> {
    return getPaged<RoleListItem>('/api/admin/identity/roles', params, {
      allowedFilterFields: ROLE_FILTER_FIELDS,
      allowedSortFields: ROLE_SORT_FIELDS,
      allowedSearchFields: ROLE_SEARCH_FIELDS,
    })
  }

  static getRole(id: string): Promise<Result<RoleDetail>> {
    return get<Result<RoleDetail>>(`/api/admin/identity/roles/${id}`)
  }

  static createRole(request: RoleRequest): Promise<Result<RoleDetail>> {
    return post<Result<RoleDetail>>('/api/admin/identity/roles', request)
  }

  static updateRole(id: string, request: RoleRequest): Promise<Result<RoleDetail>> {
    return put<Result<RoleDetail>>(`/api/admin/identity/roles/${id}`, request)
  }

  static deleteRole(id: string): Promise<Result<void>> {
    return del<Result<void>>(`/api/admin/identity/roles/${id}`)
  }

  static assignPermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return put<Result<void>>(`/api/admin/identity/roles/${id}/permissions/assign`, { permissions })
  }

  static getPermissions(id: string): Promise<Result<PermissionGroupResponse>> {
    return get<Result<PermissionGroupResponse>>(`/api/admin/identity/roles/${id}/permissions`)
  }

  static revokePermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return delWithBody<Result<void>>(`/api/admin/identity/roles/${id}/permissions/revoke`, { permissions })
  }

  static syncPermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return patch<Result<void>>(`/api/admin/identity/roles/${id}/permissions/sync`, { permissions })
  }
}