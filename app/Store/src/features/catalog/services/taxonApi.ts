import { getPaged } from '@/shared/api'
import { TaxonomyListItemSchema, TaxonListItemSchema } from '../validations/taxon'
import { PagedResultSchema } from '@/shared/validations/result'
import type { PagedResult } from '@/shared/types'
import type { StoreTaxonomyListItem, StoreTaxonListItemResponse } from '../types'
import type { QueryingParameters } from '@/shared/types/querying'

// Schema: Paged result wrappers for each endpoint response shape
const taxonomyList = PagedResultSchema(TaxonomyListItemSchema)
const taxonList = PagedResultSchema(TaxonListItemSchema)

export class TaxonomyApi {
  static async getTaxonomies(q: QueryingParameters): Promise<PagedResult<StoreTaxonomyListItem>> {
    const result = await getPaged<unknown>('/api/storefront/catalog/taxonomies', q)
    if (!result.isSuccess) return result as PagedResult<StoreTaxonomyListItem>
    const parsed = taxonomyList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreTaxonomyListItem>
  }

  static async getTaxons(q: QueryingParameters): Promise<PagedResult<StoreTaxonListItemResponse>> {
    const result = await getPaged<unknown>('/api/storefront/catalog/taxonomies/taxons', q)
    if (!result.isSuccess) return result as PagedResult<StoreTaxonListItemResponse>
    const parsed = taxonList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreTaxonListItemResponse>
  }
}
