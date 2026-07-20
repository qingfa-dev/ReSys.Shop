import apiClient from '@/common/api/http/api.client'
import { PROFILES } from '@/common/api/constants'
import type { ServerResult } from '@/common/api/types/result.types'
import type { AddressDetail } from '../types/address.response.type'
import type { CreateAddressRequest, UpdateAddressRequest } from '../types/address.request.type'
export const addressApi = {
  getAll: (userId: string): Promise<ServerResult<AddressDetail[]>> =>
    apiClient.get(`${PROFILES}/addresses`, { params: { userId } }).then(res => res.data as ServerResult<AddressDetail[]>),

  getById: (id: string): Promise<ServerResult<AddressDetail>> =>
    apiClient.get(`${PROFILES}/addresses/${id}`).then(res => res.data as ServerResult<AddressDetail>),

  create: (data: CreateAddressRequest): Promise<ServerResult<AddressDetail>> =>
    apiClient.post(`${PROFILES}/addresses`, data).then(res => res.data as ServerResult<AddressDetail>),

  update: (id: string, data: UpdateAddressRequest): Promise<ServerResult<AddressDetail>> =>
    apiClient.put(`${PROFILES}/addresses/${id}`, data).then(res => res.data as ServerResult<AddressDetail>),

  delete: (id: string, userId: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${PROFILES}/addresses/${id}`, { params: { userId } }).then(res => res.data as ServerResult<void>),
}
