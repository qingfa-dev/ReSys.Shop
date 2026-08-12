import { post, put, delWithBody } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  TaxonRuleRequest,
  TaxonRuleListItem,
  TaxonRuleDetail,
} from '../types/taxonRule'

export class TaxonRuleApi {
  static getRules(taxonId: string, params: QueryingParameters = {}): Promise<PagedResult<TaxonRuleListItem>> {
    return getPaged<TaxonRuleListItem>(`/api/admin/catalog/taxon-rules?taxonId=${taxonId}`, params)
  }

  static createRule(request: TaxonRuleRequest & { taxonId: string }): Promise<Result<TaxonRuleDetail>> {
    return post<Result<TaxonRuleDetail>>('/api/admin/catalog/taxon-rules', request)
  }

  static updateRule(ruleId: string, request: TaxonRuleRequest & { taxonId: string }): Promise<Result<TaxonRuleDetail>> {
    return put<Result<TaxonRuleDetail>>(`/api/admin/catalog/taxon-rules/${ruleId}`, request)
  }

  static deleteRule(taxonId: string, ruleId: string): Promise<Result<TaxonRuleListItem>> {
    return delWithBody<Result<TaxonRuleListItem>>(`/api/admin/catalog/taxon-rules/${ruleId}`, { taxonId, ruleId })
  }

  static syncRules(request: { taxonId: string; rules: Array<TaxonRuleRequest & { id?: string }> }): Promise<Result<TaxonRuleDetail>> {
    return post<Result<TaxonRuleDetail>>('/api/admin/catalog/taxon-rules/sync', request)
  }
}
