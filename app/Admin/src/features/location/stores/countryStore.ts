import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { CountryListItem } from '../types/country'
import { CountryApi } from '../services/countryApi'

export const useCountryStore = defineStore('countries', () => {
  const activeCountries = ref<CountryListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return

    const result = await CountryApi.getCountries({
      isActive: true,
      pageSize: 100,
      sortBy: 'name',
      sortDirection: 'asc',
    })

    if (result.isSuccess) {
      activeCountries.value = result.items
      loaded.value = true
    }
  }

  return { activeCountries, loaded, fetchActive }
})
