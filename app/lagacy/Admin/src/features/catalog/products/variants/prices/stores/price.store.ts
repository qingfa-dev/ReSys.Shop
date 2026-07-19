import { defineStore } from 'pinia'
import { ref } from 'vue'
import { priceService } from '../services/price.service'
import type { PriceRecord } from '../types/price.response.type'

export const usePriceStore = defineStore('variantPrice', () => {
  const items = ref<PriceRecord[]>([])

  async function fetchByVariant(variantId: string) {
    const result = await priceService.listPrices(variantId)
    if (result.isSuccess) items.value = result.items
    return result
  }

  return { items, fetchByVariant }
})
