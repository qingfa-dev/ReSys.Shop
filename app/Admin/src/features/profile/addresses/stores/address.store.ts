import { defineStore } from 'pinia'
import { ref } from 'vue'
import { addressService } from '../services/address.service'
import type { AddressDetail } from '../types/Address.Response.Type'

export const useAddressStore = defineStore('address', () => {
  const items = ref<AddressDetail[]>([])
  const current = ref<AddressDetail | null>(null)
  const loading = ref(false)

  async function fetchAll(userId: string) {
    loading.value = true
    const result = await addressService.getAll(userId)
    if (result.isSuccess) items.value = result.value
    loading.value = false
    return result
  }

  async function fetchById(id: string) {
    const result = await addressService.getById(id)
    if (result.isSuccess) current.value = result.value
    return result
  }

  return { items, current, loading, fetchAll, fetchById }
})
