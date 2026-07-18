import { defineStore } from 'pinia'
import { ref } from 'vue'
import { classificationService } from '../services/classification.service'
import type { ProductClassification } from '../../types/Product.Response.Type'

export const useClassificationStore = defineStore('productClassification', () => {
  const items = ref<ProductClassification[]>([])

  async function fetchByProduct(productId: string) {
    const result = await classificationService.getClassifications(productId)
    if (result.isSuccess) items.value = result.value
    return result
  }

  return { items, fetchByProduct }
})
