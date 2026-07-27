import type { RoleParameters } from '../types/role.field'
export type CreateRoleRequest = RoleParameters
export type UpdateRoleRequest = Partial<RoleParameters>

export interface AssignRolePermissionRequest {
  permissionName: string
}

export interface RevokeRolePermissionRequest {
  permissionName: string
}

export interface SyncRolePermissionsRequest {
  permissionNames: string[]
}
