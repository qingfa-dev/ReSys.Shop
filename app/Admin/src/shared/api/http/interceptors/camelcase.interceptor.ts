import type { AxiosResponse } from 'axios'
import { toCamelCaseKeys } from '@/shared/mapper/mapper.utils'

export function camelCaseInterceptor(response: AxiosResponse): AxiosResponse {
  if (response.data && typeof response.data === 'object') {
    response.data = toCamelCaseKeys(response.data as Record<string, unknown>)
  }
  return response
}
