import { addressApi } from '../api/address.api'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { AddressDetail } from '../types/address.response.type'

export const addressService = {
  getAll(userId: string): Promise<ServerResult<AddressDetail[]>> {
    return addressApi.getAll(userId)
  },

  getById(id: string): Promise<ServerResult<AddressDetail>> {
    return addressApi.getById(id)
  },

  create(data: Parameters<typeof addressApi.create>[0]): Promise<ServerResult<AddressDetail>> {
    return addressApi.create(data)
  },

  update(id: string, data: Parameters<typeof addressApi.update>[1]): Promise<ServerResult<AddressDetail>> {
    return addressApi.update(id, data)
  },

  delete: addressApi.delete,
}
