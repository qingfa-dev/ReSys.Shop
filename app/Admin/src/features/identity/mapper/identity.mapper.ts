import type { AdminUserSummary, CustomerSummary } from '../../users/types/User.Response.Type'
import type { RoleSummary } from '../../users/roles/types/Role.Response.Type'
import type { PermissionSummary } from '../../users/permissions/types/Permission.Response.Type'

export function mapAdminUserSummary(dto: AdminUserSummary): AdminUserSummary {
  return dto
}

export function mapCustomerSummary(dto: CustomerSummary): CustomerSummary {
  return dto
}

export function mapRoleSummary(dto: RoleSummary): RoleSummary {
  return dto
}

export function mapPermissionSummary(dto: PermissionSummary): PermissionSummary {
  return dto
}
