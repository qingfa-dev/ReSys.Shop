import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { CATALOG } from '@/shared/constants/api'
import { TAXON_FILTER_FIELDS, TAXON_SORT_FIELDS } from '../types/taxon'
import type { TaxonListItem } from '../types/taxon'

export function useTaxonList(options?: UsePagedQueryOptions) {
  return usePagedQuery<TaxonListItem>(`${CATALOG}/taxons`, {
    allowedFilterFields: TAXON_FILTER_FIELDS,
    allowedSortFields: TAXON_SORT_FIELDS,
    ...options,
  })
}
