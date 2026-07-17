import apiClient from '@/shared/api/api.client'
import type { ApiResult } from '@/shared/api/api.types'
import type {
  TaxonListItem,
  TaxonDetail,
  CreateTaxonRequest,
  UpdateTaxonRequest,
  TaxonQuery,
  TaxonRuleListItem,
  CreateTaxonRuleRequest,
  UpdateTaxonRuleRequest,
} from '../types/taxon.types'

const BASE_URL = '/admin/catalog/taxons'

export const taxonService = {
  async getTaxons(query?: TaxonQuery): Promise<ApiResult<TaxonListItem[]>> {
    return (await apiClient.get<TaxonListItem[]>(BASE_URL, { params: query })) as any
  },

  async getTree(query?: TaxonQuery): Promise<ApiResult<any>> {
    return (await apiClient.get<any>(`${BASE_URL}/tree`, { params: query })) as any
  },

  async getById(taxonId: string): Promise<ApiResult<TaxonDetail>> {
    return (await apiClient.get<TaxonDetail>(`${BASE_URL}/${taxonId}`)) as any
  },

  async create(request: CreateTaxonRequest): Promise<ApiResult<TaxonDetail>> {
    return (await apiClient.post<TaxonDetail>(BASE_URL, request)) as any
  },

  async update(
    taxonId: string,
    request: UpdateTaxonRequest,
  ): Promise<ApiResult<TaxonDetail>> {
    return (await apiClient.put<TaxonDetail>(
      `${BASE_URL}/${taxonId}`,
      request,
    )) as any
  },

  async delete(taxonId: string): Promise<ApiResult<void>> {
    return (await apiClient.delete<void>(`${BASE_URL}/${taxonId}`)) as any
  },

  // Taxon Rules
  async getRules(taxonId: string): Promise<ApiResult<TaxonRuleListItem[]>> {
    return (await apiClient.get<TaxonRuleListItem[]>(
      `${BASE_URL}/${taxonId}/rules`,
    )) as any
  },

  async addRule(
    taxonId: string,
    request: CreateTaxonRuleRequest,
  ): Promise<ApiResult<TaxonRuleListItem>> {
    return (await apiClient.post<TaxonRuleListItem>(
      `${BASE_URL}/${taxonId}/rules`,
      request,
    )) as any
  },

  async updateRule(
    taxonId: string,
    ruleId: string,
    request: UpdateTaxonRuleRequest,
  ): Promise<ApiResult<TaxonRuleListItem>> {
    return (await apiClient.put<TaxonRuleListItem>(
      `${BASE_URL}/${taxonId}/rules/${ruleId}`,
      request,
    )) as any
  },

  async deleteRule(taxonId: string, ruleId: string): Promise<ApiResult<void>> {
    return (await apiClient.delete<void>(
      `${BASE_URL}/${taxonId}/rules/${ruleId}`,
    )) as any
  },

  async regenerateProducts(taxonId: string): Promise<ApiResult<void>> {
    return (await apiClient.post<void>(
      `${BASE_URL}/${taxonId}/rules/regenerate`,
      {},
    )) as any
  },

  async getProductPreview(taxonId: string, params?: { page?: number, page_size?: number }): Promise<ApiResult<any>> {
    return (await apiClient.get<any>(
      `${BASE_URL}/${taxonId}/preview`,
      { params }
    )) as any
  },
}
