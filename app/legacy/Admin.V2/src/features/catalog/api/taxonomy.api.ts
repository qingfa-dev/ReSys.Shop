import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { TaxonomyResponse, CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../types'

export class TaxonomyApi {
  static getMany(query: ListQuery): Promise<PagedResult<TaxonomyResponse>> {
    return getPagedList<TaxonomyResponse>('/catalog/taxonomies', query)
  }

  static async get(id: string): Promise<Result<TaxonomyResponse>> {
    const res = await apiClient.get<Result<TaxonomyResponse>>(`/catalog/taxonomies/${id}`)
    return res.data
  }

  static async create(data: CreateTaxonomyRequest): Promise<Result<TaxonomyResponse>> {
    const res = await apiClient.post<Result<TaxonomyResponse>>('/catalog/taxonomies', data)
    return res.data
  }

  static async update(id: string, data: UpdateTaxonomyRequest): Promise<Result<TaxonomyResponse>> {
    const res = await apiClient.put<Result<TaxonomyResponse>>(`/catalog/taxonomies/${id}`, data)
    return res.data
  }

  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/taxonomies/${id}`)
    return res.data
  }

  static async restore(id: string): Promise<Result<void>> {
    const res = await apiClient.patch<Result<void>>(`/catalog/taxonomies/${id}/restore`)
    return res.data
  }
}
