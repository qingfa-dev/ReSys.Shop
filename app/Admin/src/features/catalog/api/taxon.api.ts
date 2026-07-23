import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { TaxonResponse, TaxonRequest } from '../types'

export class TaxonApi {
  static async getMany(taxonomyId: string): Promise<Result<TaxonResponse[]>> {
    const res = await apiClient.get<Result<TaxonResponse[]>>(`/catalog/taxonomies/${taxonomyId}/taxons`)
    return res.data
  }

  static async create(taxonomyId: string, data: TaxonRequest): Promise<Result<TaxonResponse>> {
    const res = await apiClient.post<Result<TaxonResponse>>(`/catalog/taxonomies/${taxonomyId}/taxons`, data)
    return res.data
  }

  static async update(taxonomyId: string, id: string, data: TaxonRequest): Promise<Result<TaxonResponse>> {
    const res = await apiClient.put<Result<TaxonResponse>>(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`, data)
    return res.data
  }

  static async delete(taxonomyId: string, id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`)
    return res.data
  }
}
