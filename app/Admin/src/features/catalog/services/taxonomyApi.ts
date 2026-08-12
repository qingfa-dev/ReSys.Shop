import { post, get, put, patch, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  TaxonomyRequest,
  TaxonomyListItem,
  TaxonomyDetail,
} from '../types/taxonomy'
import {
  TAXONOMY_FILTER_FIELDS,
  TAXONOMY_SORT_FIELDS,
} from '../types/taxonomy'

export class TaxonomyApi {
  static getTaxonomies(params: QueryingParameters): Promise<PagedResult<TaxonomyListItem>> {
    return getPaged<TaxonomyListItem>('/api/admin/catalog/taxonomies', params, {
      allowedFilterFields: TAXONOMY_FILTER_FIELDS,
      allowedSortFields: TAXONOMY_SORT_FIELDS,
      allowedSearchFields: ['name', 'presentation'],
    })
  }

  static getTaxonomy(id: string): Promise<Result<TaxonomyDetail>> {
    return get<Result<TaxonomyDetail>>(`/api/admin/catalog/taxonomies/${id}`)
  }

  static createTaxonomy(request: TaxonomyRequest): Promise<Result<TaxonomyDetail>> {
    return post<Result<TaxonomyDetail>>('/api/admin/catalog/taxonomies', request)
  }

  static updateTaxonomy(id: string, request: TaxonomyRequest): Promise<Result<TaxonomyDetail>> {
    return put<Result<TaxonomyDetail>>(`/api/admin/catalog/taxonomies/${id}`, request)
  }

  static deleteTaxonomy(id: string): Promise<Result<TaxonomyListItem>> {
    return del<Result<TaxonomyListItem>>(`/api/admin/catalog/taxonomies/${id}`)
  }

  static restoreTaxonomy(id: string): Promise<Result<void>> {
    return patch<Result<void>>(`/api/admin/catalog/taxonomies/${id}/restore`)
  }
}
