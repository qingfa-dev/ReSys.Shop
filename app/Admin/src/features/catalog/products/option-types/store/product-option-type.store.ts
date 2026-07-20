import { defineStore } from 'pinia'
import { ref } from 'vue'
import { productOptionTypeApi } from '../api/product-option-type.api'
import type { OptionTypeDetail } from '../../../option-types/types/option-type.response'

export const useProductOptionTypeStore = defineStore('productOptionType', () => {
  const items = ref<OptionTypeDetail[]>([])

  async function fetchByProduct(productId: string) {
    const result = await productOptionTypeApi.getOptionTypes(productId)
    if (result.isSuccess && result.value) items.value = result.value
    return result
  }

  return { items, fetchByProduct }
})
