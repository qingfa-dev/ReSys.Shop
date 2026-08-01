import { ref } from 'vue'
import type { Ref } from 'vue'
import type { Result } from '@/shared/types'
import type { AddressResponse } from '../types/address'
import { AddressApi } from '../services/addressApi'

export interface UseAddressDetailState {
  address: Ref<AddressResponse | null>
  loading: Ref<boolean>
  error: Ref<string | null>
  fetchAddress: (userId: string, id: string) => Promise<Result<AddressResponse>>
}

export function useAddressDetail() {
  const address = ref<AddressResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchAddress(userId: string, id: string): Promise<Result<AddressResponse>> {
    loading.value = true
    error.value = null
    const result = await AddressApi.getAddress(userId, id)
    loading.value = false
    if (result.isSuccess) {
      address.value = result.value
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }
    return result
  }

  return { address, loading, error, fetchAddress }
}
