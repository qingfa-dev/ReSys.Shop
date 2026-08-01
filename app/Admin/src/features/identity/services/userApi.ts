import { post, get, put, patch, del, delWithBody } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { IDENTITY } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  UserRequest,
  UserListItem,
  UserDetail,
  UserQuery,
} from '../types/user'
import {
  toUserQueryParams,
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
  private static readonly BASE = `${IDENTITY}/users`

  static getUsers(query: UserQuery): Promise<PagedResult<UserListItem>> {
    return getPaged<UserListItem>(UserApi.BASE, toUserQueryParams(query), {
      allowedFilterFields: USER_FILTER_FIELDS,
      allowedSortFields: USER_SORT_FIELDS,
      allowedSearchFields: USER_SEARCH_FIELDS,
    })
  }

  static getUser(id: string): Promise<Result<UserDetail>> {
    return get<Result<UserDetail>>(`${UserApi.BASE}/${id}`)
  }

  static createUser(request: UserRequest): Promise<Result<UserDetail>> {
    return post<Result<UserDetail>>(UserApi.BASE, request)
  }

  static updateUser(id: string, request: UserRequest): Promise<Result<UserDetail>> {
    return put<Result<UserDetail>>(`${UserApi.BASE}/${id}`, request)
  }

  static deleteUser(id: string): Promise<Result<void>> {
    return del<Result<void>>(`${UserApi.BASE}/${id}`)
  }

  static toggleStatus(id: string): Promise<Result<void>> {
    return patch<Result<void>>(`${UserApi.BASE}/${id}/status`)
  }

  static getRoles(id: string): Promise<PagedResult<UserRoleAssignment>> {
    return getPaged<UserRoleAssignment>(
      `${UserApi.BASE}/${id}/roles`,
      { pageNumber: 1, pageSize: 100 },
    )
  }

  static assignRoles(id: string, roleNames: string[]): Promise<Result<void>> {
    return post<Result<void>>(`${UserApi.BASE}/${id}/roles/assign`, { roles: roleNames })
  }

  static revokeRoles(id: string, roleNames: string[]): Promise<Result<void>> {
    return post<Result<void>>(`${UserApi.BASE}/${id}/roles/revoke`, { roles: roleNames })
  }

  static syncRoles(id: string, roleNames: string[]): Promise<Result<void>> {
    return patch<Result<void>>(`${UserApi.BASE}/${id}/roles/sync`, { roles: roleNames })
  }

  static getPermissions(id: string): Promise<Result<PermissionGroupResponse>> {
    return get<Result<PermissionGroupResponse>>(`${UserApi.BASE}/${id}/permissions`)
  }

  static assignPermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return post<Result<void>>(`${UserApi.BASE}/${id}/permissions/assign`, { permissions })
  }

  static revokePermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return delWithBody<Result<void>>(`${UserApi.BASE}/${id}/permissions/revoke`, { permissions })
  }

  static syncPermissions(id: string, permissions: string[]): Promise<Result<void>> {
    return put<Result<void>>(`${UserApi.BASE}/${id}/permissions/sync`, { permissions })
  }
}
