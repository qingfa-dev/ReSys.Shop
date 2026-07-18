export interface AdminUserSummary {
  id: string
  email: string
  userName: string | null
  firstName: string | null
  lastName: string | null
  phoneNumber: string | null
  emailConfirmed: boolean
  phoneNumberConfirmed: boolean
  fullName: string | null
  isActive: boolean
  createdAtUtc: string
}

export interface CustomerSummary {
  id: string; email: string; firstName: string | null; lastName: string | null
  fullName: string | null; ordersCount: number; totalSpent: number
  isActive: boolean; createdAtUtc: string
}
