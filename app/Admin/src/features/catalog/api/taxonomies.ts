import apiClient from '@/shared/api/client'
import { resultToMapped, pagedResultToMapped } from '@/shared/api/utils/result.mapper'
import type { MappedResult } from '@/shared/api/utils/result.mapper'
import type { Result, PagedResult, PaginationMeta } from '@/shared/models'
import type {
  TaxonomyResponse, TaxonomyRequest, TaxonomyListParams,
  TaxonResponse, TaxonRequest,
} from '../models/Taxonomy'

export async function getTaxonomies(
  params: TaxonomyListParams = {},
): Promise<MappedResult<TaxonomyResponse[]> & { meta?: PaginationMeta }> {
  const res = await apiClient.get<PagedResult<TaxonomyResponse>>('/catalog/taxonomies', { params })
  return pagedResultToMapped(res.data)
}

export async function getTaxonomy(id: string): Promise<MappedResult<TaxonomyResponse>> {
  const res = await apiClient.get<Result<TaxonomyResponse>>(`/catalog/taxonomies/${id}`)
  return resultToMapped(res.data)
}

export async function createTaxonomy(data: TaxonomyRequest): Promise<MappedResult<TaxonomyResponse>> {
  const res = await apiClient.post<Result<TaxonomyResponse>>('/catalog/taxonomies', data)
  return resultToMapped(res.data)
}

export async function updateTaxonomy(id: string, data: TaxonomyRequest): Promise<MappedResult<TaxonomyResponse>> {
  const res = await apiClient.put<Result<TaxonomyResponse>>(`/catalog/taxonomies/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteTaxonomy(id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/taxonomies/${id}`)
  return resultToMapped(res.data)
}

export async function getTaxons(taxonomyId: string): Promise<MappedResult<TaxonResponse[]>> {
  const res = await apiClient.get<Result<TaxonResponse[]>>(`/catalog/taxonomies/${taxonomyId}/taxons`)
  return resultToMapped(res.data)
}

export async function createTaxon(taxonomyId: string, data: TaxonRequest): Promise<MappedResult<TaxonResponse>> {
  const res = await apiClient.post<Result<TaxonResponse>>(`/catalog/taxonomies/${taxonomyId}/taxons`, data)
  return resultToMapped(res.data)
}

export async function updateTaxon(taxonomyId: string, id: string, data: TaxonRequest): Promise<MappedResult<TaxonResponse>> {
  const res = await apiClient.put<Result<TaxonResponse>>(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteTaxon(taxonomyId: string, id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`)
  return resultToMapped(res.data)
}
