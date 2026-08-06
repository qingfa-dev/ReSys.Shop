import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import { getPaged } from '@/shared/api/paged'
import type { StoreTaxonomyListItem, StoreTaxonListItemResponse } from '../types/taxon'

export function getTaxonomies(params: QueryingParameters): Promise<PagedResult<StoreTaxonomyListItem>> {
  return getPaged<StoreTaxonomyListItem>(ENDPOINTS.taxonomies, params)
}

export function getTaxons(params: QueryingParameters): Promise<PagedResult<StoreTaxonListItemResponse>> {
  return getPaged<StoreTaxonListItemResponse>(ENDPOINTS.taxons, params)
}
