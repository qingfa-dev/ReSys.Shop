import type { AdminUserSummary } from './user.response.type'

export interface AdminUserSummaryModel extends AdminUserSummary {
  hasRole: boolean
  isLocked: boolean
}

export function toAdminUserSummaryModel(dto: AdminUserSummary): AdminUserSummaryModel {
  return { ...dto, hasRole: false, isLocked: false }
}
