import { shippingMethodRepository } from '../api/shipping-method.api'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { ShippingMethodListItemModel, ShippingMethodDetailModel } from '../types/shipping-method.model.type'
import { mapShippingMethodListItem, mapShippingMethodDetail } from '../mappers/shipping-method.mapper'

export const shippingMethodService = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<ShippingMethodListItemModel>> {
    const result = await shippingMethodRepository.list(params)
    if (result.isSuccess) {
      return { ...result, items: result.items.map(mapShippingMethodListItem) }
    }
    return result as ServerPagedResult<ShippingMethodListItemModel>
  },
  async getById(id: string): Promise<ServerResult<ShippingMethodDetailModel>> {
    const result = await shippingMethodRepository.getById(id)
    if (result.isSuccess) {
      return { ...result, value: mapShippingMethodDetail(result.value) }
    }
    return result as ServerResult<ShippingMethodDetailModel>
  },
  async create(data: Record<string, unknown>): Promise<ServerResult<ShippingMethodDetailModel>> {
    const result = await shippingMethodRepository.create(data as never)
    if (result.isSuccess) {
      return { ...result, value: mapShippingMethodDetail(result.value) }
    }
    return result as ServerResult<ShippingMethodDetailModel>
  },
  async update(id: string, data: Record<string, unknown>): Promise<ServerResult<ShippingMethodDetailModel>> {
    const result = await shippingMethodRepository.update(id, data as never)
    if (result.isSuccess) {
      return { ...result, value: mapShippingMethodDetail(result.value) }
    }
    return result as ServerResult<ShippingMethodDetailModel>
  },
  delete: shippingMethodRepository.delete,
  activate: shippingMethodRepository.activate,
  deactivate: shippingMethodRepository.deactivate,
}
