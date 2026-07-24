import type { UserParameters } from '../types/user.field'
export type CreateAdminUserRequest = UserParameters
export type UpdateAdminUserRequest = Partial<CreateAdminUserRequest>

export interface AssignRoleRequest {
  roleName: string
}

export interface SyncRolesRequest {
  roleNames: string[]
}

export interface AssignPermissionRequest {
  permissionName: string
}

export interface SyncPermissionsRequest {
  permissionNames: string[]
}

export interface UpdateUserStatusRequest {
  isActive: boolean
}
