export interface UserRoleItem {
  roleId: string
  name: string
  isAssigned: boolean
}

export interface UserRoleListResponse {
  items: UserRoleItem[]
}
