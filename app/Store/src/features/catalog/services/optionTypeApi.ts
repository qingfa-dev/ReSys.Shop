import { getPaged } from '@/shared/api/paged'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type { StoreOptionTypeListItem } from '../types/optionType'

export function getOptionTypes(params: QueryingParameters): Promise<PagedResult<StoreOptionTypeListItem>> {
  return getPaged<StoreOptionTypeListItem>(ENDPOINTS.optionTypes, params)
}
