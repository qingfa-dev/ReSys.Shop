import { defineStore } from 'pinia'
import { ref } from 'vue'
import { addressApi } from '../api/address.api'
import type { AddressDetail } from '../types/address.response'

export const useAddressStore = defineStore('address', () => {
  const items = ref<AddressDetail[]>([])
  const current = ref<AddressDetail | null>(null)
  const loading = ref(false)

  async function fetchAll(userId: string) {
    loading.value = true
    const result = await addressApi.getAll(userId)
    if (result.isSuccess) items.value = result.value
    loading.value = false
    return result
  }

  async function fetchById(id: string) {
    const result = await addressApi.getById(id)
    if (result.isSuccess) current.value = result.value
    return result
  }

  return { items, current, loading, fetchAll, fetchById }
})
