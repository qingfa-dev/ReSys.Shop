import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Result, PagedResult } from '@/shared/types'
import type { TaxonDetail } from '../types/taxon'
import type { TaxonRuleListItem } from '../types/taxonRule'
import { TaxonApi } from '../services/taxonApi'
import { TaxonRuleApi } from '../services/taxonRuleApi'

export const useTaxonDetailStore = defineStore('taxonDetail', () => {
  const currentTaxon = ref<TaxonDetail | null>(null)
  const detailLoading = ref(false)
  const rules = ref<TaxonRuleListItem[]>([])
  const rulesLoading = ref(false)

  async function fetchDetail(id: string): Promise<Result<TaxonDetail>> {
    detailLoading.value = true
    const result = await TaxonApi.getTaxon(id)
    detailLoading.value = false

    if (result.isSuccess) {
      currentTaxon.value = result.value
    }

    return result
  }

  async function fetchRules(taxonId: string): Promise<PagedResult<TaxonRuleListItem>> {
    rulesLoading.value = true
    const result = await TaxonRuleApi.getRules(taxonId)
    rulesLoading.value = false

    if (result.isSuccess) {
      rules.value = result.items
    }

    return result
  }

  return { currentTaxon, detailLoading, fetchDetail, rules, rulesLoading, fetchRules }
})
