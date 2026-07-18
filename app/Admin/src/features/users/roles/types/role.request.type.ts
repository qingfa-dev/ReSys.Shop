import type { RoleParameters } from '../schemas/Role.Schema'
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
