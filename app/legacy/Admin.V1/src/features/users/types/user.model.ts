import type { AdminUserSummary } from './user.response'

export interface AdminUserSummaryModel extends AdminUserSummary {
  hasRole: boolean
  isLocked: boolean
}

export function toAdminUserSummaryModel(dto: AdminUserSummary): AdminUserSummaryModel {
  return { ...dto, hasRole: false, isLocked: false }
}
