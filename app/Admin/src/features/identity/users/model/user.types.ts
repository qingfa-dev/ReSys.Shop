import type { IsoDateString } from '@/shared/types/timestamp'
import type { UserId } from '@/shared/types/id'

export type UserStatus = 'active' | 'inactive' | 'invited' | 'suspended'

export interface User {
  id: UserId
  email: string
  displayName: string
  status: UserStatus
  roles: string[]
  createdAt: IsoDateString
  updatedAt: IsoDateString
}

export interface UserListItem {
  id: UserId
  email: string
  displayName: string
  status: UserStatus
  roleCount: number
}

export interface UserCreateRequest {
  email: string
  displayName: string
  password: string
  roleIds: string[]
}

export interface UserUpdateRequest {
  id: UserId
  displayName?: string
  status?: UserStatus
  roleIds?: string[]
}
