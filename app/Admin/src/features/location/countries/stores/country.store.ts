import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useToast } from '@/common/composables/toast.use'
import { useI18n } from 'vue-i18n'
import { countryService } from '../services/country.service'
import type { Country } from '../types/country.response.type'
import type { CreateCountryRequest, UpdateCountryRequest } from '../types/country.request.type'

export const useCountryStore = defineStore('country', () => {
  const { showToast } = useToast()
  const { t } = useI18n()
  const items = ref<Country[]>([])
  const loading = ref(false)
  const submitting = ref(false)
  const totalRecords = ref(0)

  async function fetchCountries(params?: Record<string, unknown>) {
    loading.value = true
    const result = await countryService.list(params)
    if (result.isSuccess) {
      items.value = result.items
      totalRecords.value = result.totalCount ?? result.items.length
    } else {
      showToast('error', t('common.error'), result.errors?.[0]?.message || t('location.messages.load_error'))
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

  async function createCountry(data: CreateCountryRequest) {
    submitting.value = true
    const result = await countryService.create(data)
    if (result.isSuccess) {
      showToast('success', t('common.created'), t('location.messages.create_success'))
      await fetchCountries()
    }
    submitting.value = false
    return result
  }

  async function updateCountry(id: string, data: UpdateCountryRequest) {
    submitting.value = true
    const result = await countryService.update(id, data)
    if (result.isSuccess) {
      showToast('success', t('common.updated'), t('location.messages.update_success'))
      await fetchCountries()
    }
    submitting.value = false
    return result
  }

  async function deleteCountry(id: string) {
    loading.value = true
    const result = await countryService.delete(id)
    if (result.isSuccess) {
      showToast('success', t('common.deleted'), t('location.messages.delete_success'))
      items.value = items.value.filter(i => i.id !== id)
      totalRecords.value--
    }
    loading.value = false
    return result
  }

  return { items, loading, submitting, totalRecords, fetchCountries, fetchCountryById, createCountry, updateCountry, deleteCountry }
})
