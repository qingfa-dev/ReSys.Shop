import { getPaged } from '@/shared/api/paged'
import type { PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type { StoreOptionValueListItemResponse } from '../types/optionType'

// Call: Catalog API — fetch option values for filter sidebar
export function getOptionValues(params: QueryingParameters): Promise<PagedResult<StoreOptionValueListItemResponse>> {
  return getPaged<StoreOptionValueListItemResponse>('/api/storefront/catalog/option-values', params)
}
