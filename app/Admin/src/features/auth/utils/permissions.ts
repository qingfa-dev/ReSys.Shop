export function hasPermission(required: string, userPermissions: string[]): boolean {
  return userPermissions.includes(required) || userPermissions.includes('*')
}

export function hasAnyPermission(required: string[], userPermissions: string[]): boolean {
  return required.some(p => hasPermission(p, userPermissions))
}

export function hasAllPermissions(required: string[], userPermissions: string[]): boolean {
  return required.every(p => hasPermission(p, userPermissions))
}
