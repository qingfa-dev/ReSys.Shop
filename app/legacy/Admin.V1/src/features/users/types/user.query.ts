import type { ServerQueryingParameters } from '@/common/api/types/query.types'
export interface UserQuery extends ServerQueryingParameters {
    role?: string
}
