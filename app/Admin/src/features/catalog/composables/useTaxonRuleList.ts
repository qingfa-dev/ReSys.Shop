import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import type { TaxonRuleListItem } from '../types/taxonRule'

export function useTaxonRuleList(taxonId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<TaxonRuleListItem>(() => `api/admin/catalog/taxon-rules?taxonId=${taxonId}`, {
    ...options,
  })
}
