import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { CATALOG } from '@/shared/constants/api'
import { TAXON_FILTER_FIELDS, TAXON_SORT_FIELDS } from '../types/taxon'
import type { TaxonTreeItem } from '../types/taxon'

export function useTaxonTree(taxonomyId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<TaxonTreeItem>(() => `${CATALOG}/taxons/tree?taxonomyId=${taxonomyId}`, {
    allowedFilterFields: TAXON_FILTER_FIELDS,
    allowedSortFields: TAXON_SORT_FIELDS,
    ...options,
  })
}
