import type { ServerQueryingParameters } from '@/common/api/types/query.types'

export interface TaxonQuery extends ServerQueryingParameters {
  taxonomyId?: string[]
  focusedTaxonId?: string
  includeLeavesOnly?: boolean
  includeHidden?: boolean
  maxDepth?: number
}
