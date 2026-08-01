import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { AddressResponse } from '../types/address'
import { AddressApi } from '../services/addressApi'

export const useAddressStore = defineStore('addresses', () => {
  const activeAddresses = ref<AddressResponse[]>([])
  const loaded = ref(false)

  async function fetchActive(userId: string): Promise<void> {
    if (loaded.value) return
    const result = await AddressApi.getAddresses(userId, { userId, pageSize: 100, sortBy: 'firstName', sortDirection: 'asc' })
    if (result.isSuccess) {
      activeAddresses.value = result.items
      loaded.value = true
    }
  }

  return { activeAddresses, loaded, fetchActive }
})
