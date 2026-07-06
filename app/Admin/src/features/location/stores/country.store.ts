import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useToast } from '@/shared/composables/toast.use'
import { countryService } from '../services/country.service'
import type { Country } from '../types/country.types'

export const useCountryStore = defineStore('country', () => {
  const { showToast } = useToast()
  const items = ref<Country[]>([])
  const loading = ref(false)
  const submitting = ref(false)
  const totalRecords = ref(0)

  async function fetchCountries(params?: Record<string, unknown>) {
    loading.value = true
    const result = await countryService.getAll(params)
    if (result.success) {
      items.value = result.data
      totalRecords.value = result.meta?.total_count ?? result.data.length
    } else {
      showToast('error', 'Error', result.error?.detail || 'Failed to load countries')
    }
    loading.value = false
    return result
  }

  async function fetchCountryById(id: string) {
    loading.value = true
    const result = await countryService.getById(id)
    loading.value = false
    return result
  }

  async function createCountry(data: import('../types/country.types').CountryCreateRequest) {
    submitting.value = true
    const result = await countryService.create(data)
    if (result.success) {
      showToast('success', 'Created', 'Country created successfully')
      await fetchCountries()
    }
    submitting.value = false
    return result
  }

  async function updateCountry(id: string, data: import('../types/country.types').CountryUpdateRequest) {
    submitting.value = true
    const result = await countryService.update(id, data)
    if (result.success) {
      showToast('success', 'Updated', 'Country updated successfully')
      await fetchCountries()
    }
    submitting.value = false
    return result
  }

  async function deleteCountry(id: string) {
    loading.value = true
    const result = await countryService.delete(id)
    if (result.success) {
      showToast('success', 'Deleted', 'Country removed successfully')
      items.value = items.value.filter(i => i.id !== id)
      totalRecords.value--
    }
    loading.value = false
    return result
  }

  return { items, loading, submitting, totalRecords, fetchCountries, fetchCountryById, createCountry, updateCountry, deleteCountry }
})
