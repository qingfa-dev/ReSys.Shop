export interface CreateUserRequest {
  email: string
  userName: string
  password: string
  firstName: string
  lastName: string
  phone?: string | null
  isActive?: boolean
}

export interface UpdateUserRequest {
  email: string
  firstName: string
  lastName: string
  phone?: string | null
  isActive?: boolean
}

export interface ToggleUserStatusRequest {
  isActive: boolean
}
