import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import type { Ref } from 'vue'
import { TaxonApi } from '../services/taxonApi'
import type { TaxonListItem } from '../types/taxon'

export function useTaxonList(taxonomyId?: Ref<string | null>, options?: UsePagedQueryOptions) {
  return usePagedQuery<TaxonListItem>(
    (params) =>
      taxonomyId?.value ? TaxonApi.getList(taxonomyId.value, params) : TaxonApi.getTaxons(params),
    {
      defaultSort: ['position'],
      ...options,
    },
  )
}
