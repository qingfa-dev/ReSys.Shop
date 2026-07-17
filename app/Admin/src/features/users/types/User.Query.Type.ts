import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
export interface UserQuery extends ServerQueryingParameters {
    role?: string
}
