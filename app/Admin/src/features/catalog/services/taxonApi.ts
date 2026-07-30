import { post, get, put, del } from '@/shared/api/client'
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
  private static readonly BASE = `${CATALOG}/taxonomies/taxons`

  static getTaxons(query: TaxonQuery): Promise<PagedResult<TaxonListItem>> {
    return getPaged<TaxonListItem>(TaxonApi.BASE, toTaxonQueryParams(query), {
      allowedFilterFields: TAXON_FILTER_FIELDS,
      allowedSortFields: TAXON_SORT_FIELDS,
    })
  }

  static getTaxon(id: string): Promise<Result<TaxonDetail>> {
    return get<Result<TaxonDetail>>(`${TaxonApi.BASE}/${id}`)
  }

  static getTree(): Promise<Result<{ tree: TaxonTreeItem[] }>> {
    return get<Result<{ tree: TaxonTreeItem[] }>>(`${TaxonApi.BASE}/tree`)
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
}
