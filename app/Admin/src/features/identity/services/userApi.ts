import { post, get, put, patch, del, delWithBody } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  UserRequest,
  UserListItem,
  UserDetail,
} from '../types/user'
import {
  USER_FILTER_FIELDS,
  USER_SORT_FIELDS,
  USER_SEARCH_FIELDS,
} from '../types/user'
import type { PermissionGroupResponse } from '../types/permission'
import type { RoleListItem } from '../types/role'

export interface UserRoleAssignment extends RoleListItem {
  isAssigned: boolean
}

export class UserApi {
  static getUsers(params: QueryingParameters): Promise<PagedResult<UserListItem>> {
    return getPaged<UserListItem>('/api/admin/identity/users', params, {
      allowedFilterFields: USER_FILTER_FIELDS,
      allowedSortFields: USER_SORT_FIELDS,
      allowedSearchFields: USER_SEARCH_FIELDS,
    })
  }

  static getUser(id: string): Promise<Result<UserDetail>> {
    return get<Result<UserDetail>>(`/api/admin/identity/users/${id}`)
  }

  static createUser(request: UserRequest): Promise<Result<UserDetail>> {
    return post<Result<UserDetail>>('/api/admin/identity/users', request)
  }

  static updateUser(id: string, request: UserRequest): Promise<Result<UserDetail>> {
    return put<Result<UserDetail>>(`/api/admin/identity/users/${id}`, request)
  }

  static deleteUser(id: string): Promise<Result<void>> {
    return del<Result<void>>(`/api/admin/identity/users/${id}`)
  }

  static toggleStatus(id: string): Promise<Result<void>> {
    return patch<Result<void>>(`/api/admin/identity/users/${id}/status`)
  }

  static getRoles(id: string): Promise<PagedResult<UserRoleAssignment>> {
    return getPaged<UserRoleAssignment>(
      `/api/admin/identity/users/${id}/roles`,
      { pageNumber: 1, pageSize: 100 },
    )
  }

  static assignRoles(id: string, roleNames: string[]): Promise<Result<void>> {
    return post<Result<void>>(`/api/admin/identity/users/${id}/roles/assign`, { roles: roleNames })
  }

  static revokeRoles(id: string, roleNames: string[]): Promise<Result<void>> {
    return post<Result<void>>(`/api/admin/identity/users/${id}/roles/revoke`, { roles: roleNames })
  }

  static syncRoles(id: string, roleNames: string[]): Promise<Result<void>> {
    return patch<Result<void>>(`/api/admin/identity/users/${id}/roles/sync`, { roles: roleNames })
  }

  static getPermissions(id: string): Promise<Result<PermissionGroupResponse>> {
    return get<Result<PermissionGroupResponse>>(`/api/admin/identity/users/${id}/permissions`)
  }

  static assignPermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return post<Result<void>>(`/api/admin/identity/users/${id}/permissions/assign`, { permissions })
  }

  static revokePermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return delWithBody<Result<void>>(`/api/admin/identity/users/${id}/permissions/revoke`, { permissions })
  }

  static syncPermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return put<Result<void>>(`/api/admin/identity/users/${id}/permissions/sync`, { permissions })
  }
}