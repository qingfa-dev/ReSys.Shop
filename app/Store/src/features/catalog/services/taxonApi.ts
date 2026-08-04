import { get } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import { getPaged } from '@/shared/api/paged'
import type { StoreTaxonomyTreeResponse, StoreTaxonListItemResponse } from '../types/taxon'
import type { StoreProductListItemResponse } from '../types/product'

export function getTaxonomyTree(id: string): Promise<Result<StoreTaxonomyTreeResponse>> {
  return get<Result<StoreTaxonomyTreeResponse>>(ENDPOINTS.taxonomyById(id))
}

export function getTaxons(params: QueryingParameters): Promise<PagedResult<StoreTaxonListItemResponse>> {
  return getPaged<StoreTaxonListItemResponse>(ENDPOINTS.taxons, params)
}

export function getTaxonProducts(taxonId: string, params: QueryingParameters): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(
    `${ENDPOINTS.taxonProducts}?taxonId=${taxonId}`,
    params,
  )
}
