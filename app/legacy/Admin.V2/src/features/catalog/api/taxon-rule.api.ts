import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { TaxonRuleDetailResponse, TaxonRuleListItem, TaxonRuleRequest, SyncTaxonRulesRequest, SyncTaxonRulesResponse } from '../types'

export class TaxonRuleApi {
  static async getMany(taxonomyId: string, taxonId: string): Promise<Result<TaxonRuleListItem[]>> {
    const res = await apiClient.get<Result<TaxonRuleListItem[]>>(`/catalog/taxonomies/${taxonomyId}/taxons/${taxonId}/rules`)
    return res.data
  }

  static async create(taxonomyId: string, taxonId: string, data: TaxonRuleRequest): Promise<Result<TaxonRuleDetailResponse>> {
    const res = await apiClient.post<Result<TaxonRuleDetailResponse>>(`/catalog/taxonomies/${taxonomyId}/taxons/${taxonId}/rules`, data)
    return res.data
  }

  static async update(taxonomyId: string, taxonId: string, ruleId: string, data: TaxonRuleRequest): Promise<Result<TaxonRuleDetailResponse>> {
    const res = await apiClient.put<Result<TaxonRuleDetailResponse>>(`/catalog/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/${ruleId}`, data)
    return res.data
  }

  static async delete(taxonomyId: string, taxonId: string, ruleId: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/${ruleId}`)
    return res.data
  }

  static async sync(taxonomyId: string, taxonId: string, data: SyncTaxonRulesRequest): Promise<Result<SyncTaxonRulesResponse>> {
    const res = await apiClient.post<Result<SyncTaxonRulesResponse>>(`/catalog/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/sync`, data)
    return res.data
  }
}
