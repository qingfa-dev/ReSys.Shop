import { post, get, put, patch, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  TaxonRequest,
  TaxonListItem,
  TaxonDetail,
} from '../types/taxon'
import {
  TAXON_FILTER_FIELDS,
  TAXON_SORT_FIELDS,
} from '../types/taxon'

export class TaxonApi {
  static getTaxons(params: QueryingParameters): Promise<PagedResult<TaxonListItem>> {
    return getPaged<TaxonListItem>('/api/admin/catalog/taxons', params, {
      allowedFilterFields: TAXON_FILTER_FIELDS,
      allowedSortFields: TAXON_SORT_FIELDS,
      allowedSearchFields: ['name', 'slug'],
    })
  }

  static getTaxon(id: string): Promise<Result<TaxonDetail>> {
    return get<Result<TaxonDetail>>(`/api/admin/catalog/taxons/${id}`)
  }

  static getList(taxonomyId: string, params: QueryingParameters): Promise<PagedResult<TaxonListItem>> {
    return getPaged<TaxonListItem>(`/api/admin/catalog/taxons/list?taxonomyId=${taxonomyId}`, params, {
      allowedFilterFields: TAXON_FILTER_FIELDS,
      allowedSortFields: TAXON_SORT_FIELDS,
      allowedSearchFields: ['name', 'slug'],
    })
  }

  static createTaxon(request: TaxonRequest): Promise<Result<TaxonDetail>> {
    return post<Result<TaxonDetail>>('/api/admin/catalog/taxons', request)
  }

  static updateTaxon(id: string, request: TaxonRequest): Promise<Result<TaxonDetail>> {
    return put<Result<TaxonDetail>>(`/api/admin/catalog/taxons/${id}`, request)
  }

  static deleteTaxon(id: string): Promise<Result<TaxonListItem>> {
    return del<Result<TaxonListItem>>(`/api/admin/catalog/taxons/${id}`)
  }

  static restoreTaxon(id: string): Promise<Result<TaxonListItem>> {
    return patch<Result<TaxonListItem>>(`/api/admin/catalog/taxons/${id}/restore`)
  }

  static repositionTaxon(id: string, request: TaxonRequest): Promise<Result<{ id: string }>> {
    return post<Result<{ id: string }>>(`/api/admin/catalog/taxons/${id}/reposition`, request)
  }
}
