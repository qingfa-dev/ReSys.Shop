import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { OptionTypeListItem } from '../types/optionType'
import { OptionTypeApi } from '../services/optionTypeApi'

export const useOptionTypeStore = defineStore('optionTypes', () => {
  const activeOptionTypes = ref<OptionTypeListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return

    const result = await OptionTypeApi.getOptionTypes({})

    if (result.isSuccess) {
      activeOptionTypes.value = result.items
      loaded.value = true
    }
  }

  return { activeOptionTypes, loaded, fetchActive }
})
