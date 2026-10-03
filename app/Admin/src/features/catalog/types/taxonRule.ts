import type { QueryingParameters } from '@/shared/types/querying'

export interface TaxonRuleRequest {
  type: string
  matchPolicy: string
  value: string
}

export interface TaxonRuleListItem extends TaxonRuleRequest {
  id: string
  taxonId: string
}

export type TaxonRuleDetail = TaxonRuleListItem

export interface TaxonRuleQuery {
  taxonId?: string
}

export const TAXON_RULE_TYPES = [
  'product_name',
  'product_sku',
  'product_description',
  'product_price',
  'product_weight',
  'product_available',
  'product_archived',
  'variant_price',
  'variant_sku',
  'product_status',
]

export const TAXON_RULE_MATCH_POLICIES = [
  'is_equal_to',
  'is_not_equal_to',
  'contains',
  'does_not_contain',
  'starts_with',
  'ends_with',
  'greater_than',
  'less_than',
  'greater_than_or_equal',
  'less_than_or_equal',
  'in',
  'not_in',
  'is_null',
  'is_not_null',
]

export function toTaxonRuleQueryParams(query: TaxonRuleQuery): QueryingParameters {
  const filters: string[] = []

  if (query.taxonId !== undefined && query.taxonId !== '') {
    filters.push(`taxonId=${query.taxonId}`)
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: null,
    sort: null,
    pageNumber: null,
    pageSize: null,
  }
}
