import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { TaxonRuleApi } from '../services/taxonRuleApi'
import type { TaxonRuleListItem } from '../types/taxonRule'

export function useTaxonRuleList(taxonId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<TaxonRuleListItem>((params) => TaxonRuleApi.getRules(taxonId, params), {
    ...options,
  })
}
