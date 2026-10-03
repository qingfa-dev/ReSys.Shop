export interface RoleSummary {
  id: string; name: string; displayName: string | null
  description: string | null; priority: number
  isSystem: boolean; isDefault: boolean; userCount: number
}
