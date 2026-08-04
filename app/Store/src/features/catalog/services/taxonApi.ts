import { get } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import { getPaged } from '@/shared/api/paged'
import type { StoreTaxonomyTreeResponse, StoreTaxonListItemResponse } from '../types/taxon'
import type { StoreProductListItemResponse } from '../types/product'

/**
 * Seed "Categories" taxonomy id from benchmarks/scripts/demo-seed/output/001_demo_taxonomies.json.
 * The storefront exposes no taxonomy-list endpoint (only GetTree by id), so the demo-seed id
 * is used directly. Consumers may fall back to deriving an id from the flat taxon list.
 */
export const CATEGORIES_TAXONOMY_ID = '38fcd245-9def-58ad-bdaa-fad076d353a2'

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
