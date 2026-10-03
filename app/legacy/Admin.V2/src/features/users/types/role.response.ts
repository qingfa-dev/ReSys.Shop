export interface RoleResponse {
  id: string
  name: string
  description?: string | null
  isSystem: boolean
  permissionCount?: number
  createdAt: string
  updatedAt: string
}
