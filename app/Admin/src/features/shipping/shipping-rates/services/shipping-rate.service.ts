import { shippingRateRepository } from '../api/shipping-rate.api'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { ShippingRateListItemModel, ShippingRateDetailModel } from '../types/shipping-rate.model.type'
import { mapShippingRateListItem, mapShippingRateDetail } from '../mappers/shipping-rate.mapper'

export const shippingRateService = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<ShippingRateListItemModel>> {
    const result = await shippingRateRepository.list(params)
    if (result.isSuccess) {
      return { ...result, items: result.items.map(mapShippingRateListItem) }
    }
    return result as ServerPagedResult<ShippingRateListItemModel>
  },
  async getById(id: string): Promise<ServerResult<ShippingRateDetailModel>> {
    const result = await shippingRateRepository.getById(id)
    if (result.isSuccess) {
      return { ...result, value: mapShippingRateDetail(result.value) }
    }
    return result as ServerResult<ShippingRateDetailModel>
  },
  async create(data: Record<string, unknown>): Promise<ServerResult<ShippingRateDetailModel>> {
    const result = await shippingRateRepository.create(data as never)
    if (result.isSuccess) {
      return { ...result, value: mapShippingRateDetail(result.value) }
    }
    return result as ServerResult<ShippingRateDetailModel>
  },
  async update(id: string, data: Record<string, unknown>): Promise<ServerResult<ShippingRateDetailModel>> {
    const result = await shippingRateRepository.update(id, data as never)
    if (result.isSuccess) {
      return { ...result, value: mapShippingRateDetail(result.value) }
    }
    return result as ServerResult<ShippingRateDetailModel>
  },
  delete: shippingRateRepository.delete,
}
