import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import type { Ref } from 'vue'
import { TAXON_FILTER_FIELDS, TAXON_SORT_FIELDS } from '../types/taxon'
import type { TaxonListItem } from '../types/taxon'

export function useTaxonList(taxonomyId?: Ref<string | null>, options?: UsePagedQueryOptions) {
  return usePagedQuery<TaxonListItem>(
    // Transform: Scope the taxon endpoint to the selected taxonomy when set
    () => (taxonomyId?.value ? `api/admin/catalog/taxons/list?taxonomyId=${taxonomyId.value}` : 'api/admin/catalog/taxons'),
    {
      allowedFilterFields: TAXON_FILTER_FIELDS,
      allowedSortFields: TAXON_SORT_FIELDS,
      defaultSort: ['position'],
      ...options,
    },
  )
}
