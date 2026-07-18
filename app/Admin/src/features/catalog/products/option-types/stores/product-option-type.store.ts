import { defineStore } from 'pinia'
import { ref } from 'vue'
import { productOptionTypeService } from '../services/product-option-type.service'
import type { OptionTypeDetail } from '../../../option-types/types/OptionType.Response.Type'

export const useProductOptionTypeStore = defineStore('productOptionType', () => {
  const items = ref<OptionTypeDetail[]>([])

  async function fetchByProduct(productId: string) {
    const result = await productOptionTypeService.getOptionTypes(productId)
    if (result.isSuccess) items.value = result.value
    return result
  }

  return { items, fetchByProduct }
})
