import type { Permission } from '@/shared/constants'

export function hasPermission(required: Permission, userPermissions: string[]): boolean {
  return userPermissions.includes(required) || userPermissions.includes('*')
}

export function hasAnyPermission(required: Permission[], userPermissions: string[]): boolean {
  return required.some(p => hasPermission(p, userPermissions))
}

export function hasAllPermissions(required: Permission[], userPermissions: string[]): boolean {
  return required.every(p => hasPermission(p, userPermissions))
}
