export const ROLES = {
  SUPER_ADMIN: 'SuperAdmin',
  ADMIN: 'Admin',
  MANAGER: 'Manager',
  STAFF: 'Staff',
  VIEWER: 'Viewer',
} as const

export type Role = (typeof ROLES)[keyof typeof ROLES]

export const ROLE_HIERARCHY: Record<Role, number> = {
  [ROLES.SUPER_ADMIN]: 100,
  [ROLES.ADMIN]: 80,
  [ROLES.MANAGER]: 60,
  [ROLES.STAFF]: 40,
  [ROLES.VIEWER]: 20,
}

export function hasRole(userRole: string, requiredRole: Role): boolean {
  const userLevel = ROLE_HIERARCHY[userRole as Role] ?? 0
  const requiredLevel = ROLE_HIERARCHY[requiredRole]
  return userLevel >= requiredLevel
}
