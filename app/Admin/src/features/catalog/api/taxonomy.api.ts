import apiClient from '@/shared/api/client'
import type { Result, PagedResult } from '@/shared/models'
import type { TaxonomyResponse, CreateTaxonomyRequest, UpdateTaxonomyRequest, TaxonomyListParams, TaxonResponse, TaxonRequest } from '../types'

export class TaxonomyApi {
  static async getMany(params: TaxonomyListParams = {}): Promise<PagedResult<TaxonomyResponse>> {
    const res = await apiClient.get<PagedResult<TaxonomyResponse>>('/catalog/taxonomies', { params })
    return res.data
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

  static async getTaxons(taxonomyId: string): Promise<Result<TaxonResponse[]>> {
    const res = await apiClient.get<Result<TaxonResponse[]>>(`/catalog/taxonomies/${taxonomyId}/taxons`)
    return res.data
  }

  static async createTaxon(taxonomyId: string, data: TaxonRequest): Promise<Result<TaxonResponse>> {
    const res = await apiClient.post<Result<TaxonResponse>>(`/catalog/taxonomies/${taxonomyId}/taxons`, data)
    return res.data
  }

  static async updateTaxon(taxonomyId: string, id: string, data: TaxonRequest): Promise<Result<TaxonResponse>> {
    const res = await apiClient.put<Result<TaxonResponse>>(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`, data)
    return res.data
  }

  static async deleteTaxon(taxonomyId: string, id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`)
    return res.data
  }
}
