export function mapJwtToProfile(jwt: Record<string, unknown>): { id: string; email: string; roles: string[] } {
  return {
    id: String(jwt.sub || jwt.nameid || ''),
    email: String(jwt.email || ''),
    roles: Array.isArray(jwt.role) ? jwt.role : jwt.role ? [String(jwt.role)] : [],
  }
}
