import type { ServerQueryingParameters } from '@/common/api/types/query.types'

export interface OrderQuery extends ServerQueryingParameters {
    state?: string
}
