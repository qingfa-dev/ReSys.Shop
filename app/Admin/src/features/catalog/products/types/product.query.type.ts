import type { ServerQueryingParameters } from '@/common/api/types/query.types'
export interface ProductQuery extends ServerQueryingParameters {
  status?: string; taxonId?: string; season?: string
}
