import { useActiveList } from '@/shared/composables'
import type { TaxonomyListItem } from '../types/taxonomy'
import { TaxonomyApi } from '../services/taxonomyApi'

export function useActiveTaxonomies() {
  // Call: Catalog service — taxonomies for filter and form Selects
  return useActiveList<TaxonomyListItem>(() => TaxonomyApi.getTaxonomies({}))
}
