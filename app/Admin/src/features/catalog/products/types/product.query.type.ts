import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
export interface ProductQuery extends ServerQueryingParameters {
  status?: string; taxonId?: string; season?: string
}
