export interface UserRoleInfo {
  id: string
  name: string
}

export interface UserResponse {
  id: string
  email: string
  userName: string
  firstName: string
  lastName: string
  phone?: string | null
  isActive: boolean
  roles: UserRoleInfo[]
  createdAt: string
  updatedAt: string
}
