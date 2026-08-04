import { post, put, delWithBody } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  TaxonRuleRequest,
  TaxonRuleListItem,
  TaxonRuleDetail,
} from '../types/taxonRule'
import {
  toTaxonRuleQueryParams,
} from '../types/taxonRule'

export class TaxonRuleApi {
  private static readonly BASE = `${CATALOG}/taxon-rules`

  static getRules(taxonId: string): Promise<PagedResult<TaxonRuleListItem>> {
    return getPaged<TaxonRuleListItem>(`${TaxonRuleApi.BASE}?taxonId=${taxonId}`, toTaxonRuleQueryParams({ taxonId }))
  }

  static createRule(request: TaxonRuleRequest & { taxonId: string }): Promise<Result<TaxonRuleDetail>> {
    return post<Result<TaxonRuleDetail>>(TaxonRuleApi.BASE, request)
  }

  static updateRule(ruleId: string, request: TaxonRuleRequest & { taxonId: string }): Promise<Result<TaxonRuleDetail>> {
    return put<Result<TaxonRuleDetail>>(`${TaxonRuleApi.BASE}/${ruleId}`, request)
  }

  static deleteRule(taxonId: string, ruleId: string): Promise<Result<TaxonRuleListItem>> {
    return delWithBody<Result<TaxonRuleListItem>>(`${TaxonRuleApi.BASE}/${ruleId}`, { taxonId, ruleId })
  }

  static syncRules(request: { taxonId: string; rules: Array<TaxonRuleRequest & { id?: string }> }): Promise<Result<TaxonRuleDetail>> {
    return post<Result<TaxonRuleDetail>>(`${TaxonRuleApi.BASE}/sync`, request)
  }
}
