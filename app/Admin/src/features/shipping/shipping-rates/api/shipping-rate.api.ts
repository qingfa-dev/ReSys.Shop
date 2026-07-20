import apiClient from '@/common/api/http/api.client'
import { SHIPPING } from '@/common/api/constants'
import type { ServerPagedResult, ServerResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type {
  ShippingRateListItem,
  ShippingRateDetail,
  CreateShippingRateRequest,
  UpdateShippingRateRequest,
} from '../types'
import type { ShippingRateListItemModel, ShippingRateDetailModel } from '../types/shipping-rate.model.type'
import { mapValue, mapItems } from '@/common/utils/transform'
import { decimalToDisplay } from '@/shared/utils/currency'

function ratesPath(sub?: string): string {
  return `${SHIPPING}/shipping-rates${sub ? `/${sub}` : ''}`
}

export const shippingRateRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<ShippingRateListItemModel>> {
    const result = await apiClient.get(ratesPath(), { params }).then(res => res.data as ServerPagedResult<ShippingRateListItem>)
    if (result.isSuccess) {
      return mapItems(result, d => ({ ...d, costDisplay: decimalToDisplay(d.cost, d.currency) }))
    }
    return result as ServerPagedResult<ShippingRateListItemModel>
  },

  async getById(id: string): Promise<ServerResult<ShippingRateDetailModel>> {
    const result = await apiClient.get(ratesPath(id)).then(res => res.data as ServerResult<ShippingRateDetail>)
    if (result.isSuccess) {
      return mapValue(result, d => ({ ...d, costDisplay: decimalToDisplay(d.cost, d.currency) }))
    }
    return result as ServerResult<ShippingRateDetailModel>
  },

  async create(data: CreateShippingRateRequest): Promise<ServerResult<ShippingRateDetailModel>> {
    const result = await apiClient.post(ratesPath(), data).then(res => res.data as ServerResult<ShippingRateDetail>)
    if (result.isSuccess) {
      return mapValue(result, d => ({ ...d, costDisplay: decimalToDisplay(d.cost, d.currency) }))
    }
    return result as ServerResult<ShippingRateDetailModel>
  },

  async update(id: string, data: UpdateShippingRateRequest): Promise<ServerResult<ShippingRateDetailModel>> {
    const result = await apiClient.put(ratesPath(id), data).then(res => res.data as ServerResult<ShippingRateDetail>)
    if (result.isSuccess) {
      return mapValue(result, d => ({ ...d, costDisplay: decimalToDisplay(d.cost, d.currency) }))
    }
    return result as ServerResult<ShippingRateDetailModel>
  },

  delete(id: string): Promise<ServerResult<void>> {
    return apiClient.delete(ratesPath(id)).then(res => res.data as ServerResult<void>)
  },
}
