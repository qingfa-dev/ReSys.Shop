import type { User, UserListItem } from './user.types'

export function mapUser(u: User): User {
  return u
}

export function mapUserListItem(u: User): UserListItem {
  return {
    id: u.id,
    email: u.email,
    displayName: u.displayName,
    status: u.status,
    roleCount: u.roles.length,
  }
}
