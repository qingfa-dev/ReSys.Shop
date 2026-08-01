import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { CATALOG } from '@/shared/constants/api'
import type { TaxonRuleListItem } from '../types/taxonRule'

export function useTaxonRuleList(taxonId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<TaxonRuleListItem>(() => `${CATALOG}/taxon-rules?taxonId=${taxonId}`, {
    ...options,
  })
}
