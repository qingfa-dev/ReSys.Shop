import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import { TaxonListItemSchema, TaxonomyGroupSchema } from '../validations/taxon'
import { PagedResultSchema } from '@/shared/validations/result'
import type { PagedResult } from '@/shared/types'
import type { StoreTaxonListItemResponse, StoreTaxonomyListItem, TaxonomyGroup } from '../types'
import type { QueryingParameters } from '@/shared/types/querying'

const taxonList = PagedResultSchema(TaxonListItemSchema)
const taxonomyList = PagedResultSchema(TaxonomyGroupSchema)

export class TaxonApi {
  static async getTaxonomies(q: QueryingParameters): Promise<PagedResult<StoreTaxonomyListItem>> {
    const result = await getPaged<unknown>(`${CATALOG}/taxonomies`, q)
    if (!result.isSuccess) return result as PagedResult<StoreTaxonomyListItem>
    return result as PagedResult<StoreTaxonomyListItem>
  }

  static async getTaxons(q: QueryingParameters): Promise<PagedResult<StoreTaxonListItemResponse>> {
    const result = await getPaged<unknown>(`${CATALOG}/taxons`, q)
    if (!result.isSuccess) return result as PagedResult<StoreTaxonListItemResponse>
    const parsed = taxonList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreTaxonListItemResponse>
  }
}
