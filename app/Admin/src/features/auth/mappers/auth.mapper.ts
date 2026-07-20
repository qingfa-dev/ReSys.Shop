import type { UserProfile } from '../types/login.response.type'

export function mapJwtToProfile(jwt: Record<string, unknown>): { id: string; email: string; roles: string[] } {
  return {
    id: String(jwt.sub || jwt.nameid || ''),
    email: String(jwt.email || ''),
    roles: Array.isArray(jwt.role) ? jwt.role : jwt.role ? [String(jwt.role)] : [],
  }
}

export function mapProfileResponse(value: object): Partial<UserProfile> {
  const v = value as Record<string, unknown>
  return {
    id: String(v.id || v.Id || ''),
    email: String(v.email || v.Email || ''),
    fullName: String(v.fullName || v.FullName || ''),
    roles: Array.isArray(v.roles) ? v.roles.map(String) : [],
  }
}

export function mapSessionResponse(value: { id: string; roles: string[]; permissions?: string[] }): { id: string; roles: string[]; permissions: string[] } {
  return { id: value.id, roles: Array.isArray(value.roles) ? value.roles : [], permissions: Array.isArray(value.permissions) ? value.permissions : [] }
}
