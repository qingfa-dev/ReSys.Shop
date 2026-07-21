import { defineStore } from 'pinia'
import { ref } from 'vue'
import { productClassificationApi } from '../api/product-classification.api'
import type { ProductClassification } from '../models/classification.response'

export const useClassificationStore = defineStore('productClassification', () => {
  const items = ref<ProductClassification[]>([])

  async function fetchByProduct(productId: string) {
    const result = await productClassificationApi.getClassifications(productId)
    if (result.isSuccess && result.value) items.value = result.value
    return result
  }

  return { items, fetchByProduct }
})
