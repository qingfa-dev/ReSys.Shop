export interface AdminUserSummary {
  id: string; email: string; userName: string | null; firstName: string | null
  lastName: string | null; fullName: string | null; roleNames: string[]
  isActive: boolean; createdAtUtc: string
  phoneNumber?: string | null; emailConfirmed?: boolean; phoneNumberConfirmed?: boolean
  accessFailedCount?: number; lockoutEnd?: string | null
  lastSignInAtUtc?: string | null; lastIpAddress?: string | null
}

export interface CustomerSummary {
  id: string; email: string; firstName: string | null; lastName: string | null
  fullName: string | null; ordersCount: number; totalSpent: number
  isActive: boolean; createdAtUtc: string
}
