import apiClient from '@/shared/api/http/api.client'
import { SHIPPING } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface ShippingRateListItem {
  id: string
  name: string
  methodId: string
  minOrderAmount: number
  maxOrderAmount: number
  rate: number
  currency: string
}

export interface ShippingRateDetail extends ShippingRateListItem {
  conditions: Record<string, unknown> | null
}

export interface CreateShippingRateRequest {
  name: string
  methodId: string
  minOrderAmount: number
  maxOrderAmount: number
  rate: number
  currency: string
  conditions?: Record<string, unknown>
}

function ratesPath(sub?: string): string {
  return `${SHIPPING}/shipping-rates${sub ? `/${sub}` : ''}`
}

export const shippingRateRepository = {
  list(params?: ServerQueryingParameters): Promise<ServerResult<ShippingRateListItem[]>> {
    return apiClient.get(ratesPath(), { params }).then(res => res.data as ServerResult<ShippingRateListItem[]>)
  },

  getById(id: string): Promise<ServerResult<ShippingRateDetail>> {
    return apiClient.get(ratesPath(id)).then(res => res.data as ServerResult<ShippingRateDetail>)
  },

  create(data: CreateShippingRateRequest): Promise<ServerResult<ShippingRateDetail>> {
    return apiClient.post(ratesPath(), data).then(res => res.data as ServerResult<ShippingRateDetail>)
  },

  update(id: string, data: Partial<CreateShippingRateRequest>): Promise<ServerResult<ShippingRateDetail>> {
    return apiClient.put(ratesPath(id), data).then(res => res.data as ServerResult<ShippingRateDetail>)
  },

  delete(id: string): Promise<ServerResult<void>> {
    return apiClient.delete(ratesPath(id)).then(res => res.data as ServerResult<void>)
  },
}
