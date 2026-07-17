import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
export interface UserQuery extends ServerQueryingParameters {
  isActive?: boolean; role?: string
}
