import apiClient from '@/shared/api/client'
import { resultToMapped, pagedResultToMapped } from '@/shared/api/utils/result.mapper'
import type { MappedResult } from '@/shared/api/utils/result.mapper'
import type { Result, PagedResult, PaginationMeta } from '@/shared/models'
import type {
  OptionTypeResponse, OptionTypeRequest, OptionTypeListParams,
  OptionValueResponse, OptionValueRequest,
} from '../models/OptionType'

export async function getOptionTypes(
  params: OptionTypeListParams = {},
): Promise<MappedResult<OptionTypeResponse[]> & { meta?: PaginationMeta }> {
  const res = await apiClient.get<PagedResult<OptionTypeResponse>>('/catalog/option-types', { params })
  return pagedResultToMapped(res.data)
}

export async function getOptionType(id: string): Promise<MappedResult<OptionTypeResponse>> {
  const res = await apiClient.get<Result<OptionTypeResponse>>(`/catalog/option-types/${id}`)
  return resultToMapped(res.data)
}

export async function createOptionType(data: OptionTypeRequest): Promise<MappedResult<OptionTypeResponse>> {
  const res = await apiClient.post<Result<OptionTypeResponse>>('/catalog/option-types', data)
  return resultToMapped(res.data)
}

export async function updateOptionType(id: string, data: OptionTypeRequest): Promise<MappedResult<OptionTypeResponse>> {
  const res = await apiClient.put<Result<OptionTypeResponse>>(`/catalog/option-types/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteOptionType(id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/option-types/${id}`)
  return resultToMapped(res.data)
}

export async function getOptionValues(optionTypeId: string): Promise<MappedResult<OptionValueResponse[]>> {
  const res = await apiClient.get<Result<OptionValueResponse[]>>(`/catalog/option-types/${optionTypeId}/values`)
  return resultToMapped(res.data)
}

export async function createOptionValue(optionTypeId: string, data: OptionValueRequest): Promise<MappedResult<OptionValueResponse>> {
  const res = await apiClient.post<Result<OptionValueResponse>>(`/catalog/option-types/${optionTypeId}/values`, data)
  return resultToMapped(res.data)
}

export async function updateOptionValue(optionTypeId: string, id: string, data: OptionValueRequest): Promise<MappedResult<OptionValueResponse>> {
  const res = await apiClient.put<Result<OptionValueResponse>>(`/catalog/option-types/${optionTypeId}/values/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteOptionValue(optionTypeId: string, id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/option-types/${optionTypeId}/values/${id}`)
  return resultToMapped(res.data)
}
