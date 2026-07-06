import apiClient from '@/shared/api/api.client'
import type { ApiResult } from '@/shared/api/api.types'
import type { Country, CountryCreateRequest, CountryUpdateRequest } from '../types/country.types'

export const countryService = {
  async getAll(params?: Record<string, unknown>): Promise<ApiResult<Country[]>> {
    return apiClient.get('/api/admin/location/countries', { params })
  },
  async getById(id: string): Promise<ApiResult<Country>> {
    return apiClient.get(`/api/admin/location/countries/${id}`)
  },
  async create(data: CountryCreateRequest): Promise<ApiResult<Country>> {
    return apiClient.post('/api/admin/location/countries', data)
  },
  async update(id: string, data: CountryUpdateRequest): Promise<ApiResult<Country>> {
    return apiClient.put(`/api/admin/location/countries/${id}`, data)
  },
  async delete(id: string): Promise<ApiResult<void>> {
    return apiClient.delete(`/api/admin/location/countries/${id}`)
  },
}
