export interface UserPermissionItem {
  permissionId: string
  name: string
  isAssigned: boolean
}

export interface UserPermissionListResponse {
  items: UserPermissionItem[]
}
