import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { CountryResponse, CreateCountryRequest, UpdateCountryRequest } from '../types'

export class CountryApi {
  static getMany(query: ListQuery): Promise<PagedResult<CountryResponse>> {
    return getPagedList<CountryResponse>('/locations/countries', query)
  }
  static async get(id: string): Promise<Result<CountryResponse>> {
    const res = await apiClient.get<Result<CountryResponse>>(`/locations/countries/${id}`)
    return res.data
  }
  static async getByIso(isoCode: string): Promise<Result<CountryResponse>> {
    const res = await apiClient.get<Result<CountryResponse>>(`/locations/countries/by-iso/${isoCode}`)
    return res.data
  }
  static async create(data: CreateCountryRequest): Promise<Result<CountryResponse>> {
    const res = await apiClient.post<Result<CountryResponse>>('/locations/countries', data)
    return res.data
  }
  static async update(id: string, data: UpdateCountryRequest): Promise<Result<CountryResponse>> {
    const res = await apiClient.put<Result<CountryResponse>>(`/locations/countries/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/locations/countries/${id}`)
    return res.data
  }
}
