import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { AddressApi } from '../services/addressApi'
import type { AddressResponse } from '../types/address'

export function useAddressList(userId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<AddressResponse>((params) => AddressApi.getAddresses(userId, params), {
    ...options,
  })
}