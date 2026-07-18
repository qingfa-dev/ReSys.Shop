import apiClient from '@/shared/api/http/api.client'
import { SHIPPING } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface ShippingMethodListItem {
  id: string
  name: string
  description: string | null
  isActive: boolean
}

export interface ShippingMethodDetail extends ShippingMethodListItem {
  configuration: Record<string, unknown> | null
}

export interface CreateShippingMethodRequest {
  name: string
  description?: string
  configuration?: Record<string, unknown>
}

function methodsPath(sub?: string): string {
  return `${SHIPPING}/shipping-methods${sub ? `/${sub}` : ''}`
}

export const shippingMethodRepository = {
  list(params?: ServerQueryingParameters): Promise<ServerResult<ShippingMethodListItem[]>> {
    return apiClient.get(methodsPath(), { params }).then(res => res.data as ServerResult<ShippingMethodListItem[]>)
  },

  getById(id: string): Promise<ServerResult<ShippingMethodDetail>> {
    return apiClient.get(methodsPath(id)).then(res => res.data as ServerResult<ShippingMethodDetail>)
  },

  create(data: CreateShippingMethodRequest): Promise<ServerResult<ShippingMethodDetail>> {
    return apiClient.post(methodsPath(), data).then(res => res.data as ServerResult<ShippingMethodDetail>)
  },

  update(id: string, data: Partial<CreateShippingMethodRequest>): Promise<ServerResult<ShippingMethodDetail>> {
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
