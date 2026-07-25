import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { AddressResponse, CreateAddressRequest, UpdateAddressRequest } from '../types'

export class AddressApi {
  static getMany(query: ListQuery): Promise<PagedResult<AddressResponse>> {
    return getPagedList<AddressResponse>('/profiles/addresses', query)
  }
  static async get(id: string): Promise<Result<AddressResponse>> {
    const res = await apiClient.get<Result<AddressResponse>>(`/profiles/addresses/${id}`)
    return res.data
  }
  static async create(data: CreateAddressRequest): Promise<Result<AddressResponse>> {
    const res = await apiClient.post<Result<AddressResponse>>('/profiles/addresses', data)
    return res.data
  }
  static async update(id: string, data: UpdateAddressRequest): Promise<Result<AddressResponse>> {
    const res = await apiClient.put<Result<AddressResponse>>(`/profiles/addresses/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/profiles/addresses/${id}`)
    return res.data
  }
}
