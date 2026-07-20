import { defineStore } from 'pinia'
import { ref } from 'vue'
import { priceApi } from '../api/price.api'
import type { PriceRecord } from '../types/price.response'

export const usePriceStore = defineStore('variantPrice', () => {
  const items = ref<PriceRecord[]>([])

  async function fetchByVariant(variantId: string) {
    const result = await priceApi.listPrices(variantId)
    if (result.isSuccess) items.value = result.items
    return result
  }

  return { items, fetchByVariant }
})
