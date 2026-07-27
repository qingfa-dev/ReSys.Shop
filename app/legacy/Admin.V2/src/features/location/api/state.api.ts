import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { StateResponse, CreateStateRequest, UpdateStateRequest } from '../types'

export class StateApi {
  static getMany(query: ListQuery): Promise<PagedResult<StateResponse>> {
    return getPagedList<StateResponse>('/locations/states', query)
  }
  static async get(id: string): Promise<Result<StateResponse>> {
    const res = await apiClient.get<Result<StateResponse>>(`/locations/states/${id}`)
    return res.data
  }
  static async getByIso(isoCode: string): Promise<Result<StateResponse>> {
    const res = await apiClient.get<Result<StateResponse>>(`/locations/states/by-iso/${isoCode}`)
    return res.data
  }
  static async create(data: CreateStateRequest): Promise<Result<StateResponse>> {
    const res = await apiClient.post<Result<StateResponse>>('/locations/states', data)
    return res.data
  }
  static async update(id: string, data: UpdateStateRequest): Promise<Result<StateResponse>> {
    const res = await apiClient.put<Result<StateResponse>>(`/locations/states/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/locations/states/${id}`)
    return res.data
  }
}
