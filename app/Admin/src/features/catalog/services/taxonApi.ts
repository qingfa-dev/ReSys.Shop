import { post, get, put, patch, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  TaxonRequest,
  TaxonListItem,
  TaxonDetail,
  TaxonTreeItem,
  TaxonQuery,
} from '../types/taxon'
import {
  toTaxonQueryParams,
  TAXON_FILTER_FIELDS,
  TAXON_SORT_FIELDS,
} from '../types/taxon'

export class TaxonApi {
  private static readonly BASE = `${CATALOG}/taxons`

  static getTaxons(query: TaxonQuery): Promise<PagedResult<TaxonListItem>> {
    return getPaged<TaxonListItem>(TaxonApi.BASE, toTaxonQueryParams(query), {
      allowedFilterFields: TAXON_FILTER_FIELDS,
      allowedSortFields: TAXON_SORT_FIELDS,
    })
  }

  static getTaxon(id: string): Promise<Result<TaxonDetail>> {
    return get<Result<TaxonDetail>>(`${TaxonApi.BASE}/${id}`)
  }

  static getTree(taxonomyId: string): Promise<PagedResult<TaxonTreeItem>> {
    return getPaged<TaxonTreeItem>(`${TaxonApi.BASE}/tree?taxonomyId=${taxonomyId}`, toTaxonQueryParams({ taxonomyId }), {
      allowedFilterFields: TAXON_FILTER_FIELDS,
      allowedSortFields: TAXON_SORT_FIELDS,
    })
  }

  static getList(taxonomyId: string, query: TaxonQuery): Promise<PagedResult<TaxonListItem>> {
    return getPaged<TaxonListItem>(`${TaxonApi.BASE}/list?taxonomyId=${taxonomyId}`, toTaxonQueryParams(query), {
      allowedFilterFields: TAXON_FILTER_FIELDS,
      allowedSortFields: TAXON_SORT_FIELDS,
    })
  }

  static createTaxon(request: TaxonRequest): Promise<Result<TaxonDetail>> {
    return post<Result<TaxonDetail>>(TaxonApi.BASE, request)
  }

  static updateTaxon(id: string, request: TaxonRequest): Promise<Result<TaxonDetail>> {
    return put<Result<TaxonDetail>>(`${TaxonApi.BASE}/${id}`, request)
  }

  static deleteTaxon(id: string): Promise<Result<TaxonListItem>> {
    return del<Result<TaxonListItem>>(`${TaxonApi.BASE}/${id}`)
  }

  static restoreTaxon(id: string): Promise<Result<TaxonListItem>> {
    return patch<Result<TaxonListItem>>(`${TaxonApi.BASE}/${id}/restore`)
  }

  static repositionTaxon(id: string, request: TaxonRequest): Promise<Result<{ id: string }>> {
    return post<Result<{ id: string }>>(`${TaxonApi.BASE}/${id}/reposition`, request)
  }
}
