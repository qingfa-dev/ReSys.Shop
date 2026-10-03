import { ref } from 'vue'
import type { Ref } from 'vue'
import type { Result, PagedResult } from '@/shared/types'
import type { TaxonDetail } from '../types/taxon'
import type { TaxonRuleListItem } from '../types/taxonRule'
import { TaxonApi } from '../services/taxonApi'
import { TaxonRuleApi } from '../services/taxonRuleApi'

export interface UseTaxonDetailState {
  currentTaxon: Ref<TaxonDetail | null>
  detailLoading: Ref<boolean>
  rules: Ref<TaxonRuleListItem[]>
  rulesLoading: Ref<boolean>
  fetchDetail: (id: string) => Promise<Result<TaxonDetail>>
  fetchRules: (taxonId: string) => Promise<PagedResult<TaxonRuleListItem>>
}

export function useTaxonDetail(): UseTaxonDetailState {
  const currentTaxon = ref<TaxonDetail | null>(null)
  const detailLoading = ref(false)
  const rules = ref<TaxonRuleListItem[]>([])
  const rulesLoading = ref(false)

  async function fetchDetail(id: string): Promise<Result<TaxonDetail>> {
    detailLoading.value = true
    // Call: Catalog service — taxon detail that backs the edit form
    const result = await TaxonApi.getTaxon(id)
    detailLoading.value = false
    if (result.isSuccess) {
      currentTaxon.value = result.value
    }
    return result
  }

  async function fetchRules(taxonId: string): Promise<PagedResult<TaxonRuleListItem>> {
    rulesLoading.value = true
    // Call: Catalog service — taxon rules for the Rules tab
    const result = await TaxonRuleApi.getRules(taxonId)
    rulesLoading.value = false
    if (result.isSuccess) {
      rules.value = result.items
    }
    return result
  }

  return { currentTaxon, detailLoading, fetchDetail, rules, rulesLoading, fetchRules }
}
