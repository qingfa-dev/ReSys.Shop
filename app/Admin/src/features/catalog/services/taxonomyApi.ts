import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  TaxonomyRequest,
  TaxonomyListItem,
  TaxonomyDetail,
  TaxonomyQuery,
} from '../types/taxonomy'
import {
  toTaxonomyQueryParams,
  TAXONOMY_FILTER_FIELDS,
  TAXONOMY_SORT_FIELDS,
} from '../types/taxonomy'

export class TaxonomyApi {
  private static readonly BASE = `${CATALOG}/taxonomies`

  static getTaxonomies(query: TaxonomyQuery): Promise<PagedResult<TaxonomyListItem>> {
    return getPaged<TaxonomyListItem>(TaxonomyApi.BASE, toTaxonomyQueryParams(query), {
      allowedFilterFields: TAXONOMY_FILTER_FIELDS,
      allowedSortFields: TAXONOMY_SORT_FIELDS,
    })
  }

  static getTaxonomy(id: string): Promise<Result<TaxonomyDetail>> {
    return get<Result<TaxonomyDetail>>(`${TaxonomyApi.BASE}/${id}`)
  }

  static createTaxonomy(request: TaxonomyRequest): Promise<Result<TaxonomyDetail>> {
    return post<Result<TaxonomyDetail>>(TaxonomyApi.BASE, request)
  }

  static updateTaxonomy(id: string, request: TaxonomyRequest): Promise<Result<TaxonomyDetail>> {
    return put<Result<TaxonomyDetail>>(`${TaxonomyApi.BASE}/${id}`, request)
  }

  static deleteTaxonomy(id: string): Promise<Result<TaxonomyListItem>> {
    return del<Result<TaxonomyListItem>>(`${TaxonomyApi.BASE}/${id}`)
  }
}
