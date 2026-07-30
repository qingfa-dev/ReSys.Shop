import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  TaxonRuleRequest,
  TaxonRuleListItem,
  TaxonRuleDetail,
  TaxonRuleQuery,
} from '../types/taxonRule'
import {
  toTaxonRuleQueryParams,
} from '../types/taxonRule'

export class TaxonRuleApi {
  private static readonly BASE = `${CATALOG}/taxonomies/taxons`

  static getRules(taxonId: string): Promise<PagedResult<TaxonRuleListItem>> {
    const query: TaxonRuleQuery = { taxonId }
    return getPaged<TaxonRuleListItem>(`${TaxonRuleApi.BASE}/${taxonId}/rules`, toTaxonRuleQueryParams(query))
  }

  static createRule(taxonId: string, request: TaxonRuleRequest): Promise<Result<TaxonRuleDetail>> {
    return post<Result<TaxonRuleDetail>>(`${TaxonRuleApi.BASE}/${taxonId}/rules`, request)
  }

  static updateRule(taxonId: string, ruleId: string, request: TaxonRuleRequest): Promise<Result<TaxonRuleDetail>> {
    return put<Result<TaxonRuleDetail>>(`${TaxonRuleApi.BASE}/${taxonId}/rules/${ruleId}`, request)
  }

  static deleteRule(taxonId: string, ruleId: string): Promise<Result<TaxonRuleListItem>> {
    return del<Result<TaxonRuleListItem>>(`${TaxonRuleApi.BASE}/${taxonId}/rules/${ruleId}`)
  }
}
