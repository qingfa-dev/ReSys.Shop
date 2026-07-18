import apiClient from '@/shared/api/http/api.client'
import { SHIPPING } from '@/shared/api/constants'
import type { ServerPagedResult, ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type {
  ShippingMethodListItem,
  ShippingMethodDetail,
  CreateShippingMethodRequest,
  UpdateShippingMethodRequest,
} from '../types'

function methodsPath(sub?: string): string {
  return `${SHIPPING}/shipping-methods${sub ? `/${sub}` : ''}`
}

export const shippingMethodRepository = {
  list(params?: ServerQueryingParameters): Promise<ServerPagedResult<ShippingMethodListItem>> {
    return apiClient.get(methodsPath(), { params }).then(res => res.data as ServerPagedResult<ShippingMethodListItem>)
  },

  getById(id: string): Promise<ServerResult<ShippingMethodDetail>> {
    return apiClient.get(methodsPath(id)).then(res => res.data as ServerResult<ShippingMethodDetail>)
  },

  create(data: CreateShippingMethodRequest): Promise<ServerResult<ShippingMethodDetail>> {
    return apiClient.post(methodsPath(), data).then(res => res.data as ServerResult<ShippingMethodDetail>)
  },

  update(id: string, data: UpdateShippingMethodRequest): Promise<ServerResult<ShippingMethodDetail>> {
    return apiClient.put(methodsPath(id), data).then(res => res.data as ServerResult<ShippingMethodDetail>)
  },

  delete(id: string): Promise<ServerResult<void>> {
    return apiClient.delete(methodsPath(id)).then(res => res.data as ServerResult<void>)
  },

  activate(id: string): Promise<ServerResult<void>> {
    return apiClient.patch(methodsPath(`${id}/activate`)).then(res => res.data as ServerResult<void>)
  },

  deactivate(id: string): Promise<ServerResult<void>> {
    return apiClient.patch(methodsPath(`${id}/deactivate`)).then(res => res.data as ServerResult<void>)
  },
}
