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
import type { ShippingMethodListItemModel, ShippingMethodDetailModel } from '../types/shipping-method.model.type'
import { mapValue, mapItems } from '@/shared/utils/transform'

function methodsPath(sub?: string): string {
  return `${SHIPPING}/shipping-methods${sub ? `/${sub}` : ''}`
}

export const shippingMethodRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<ShippingMethodListItemModel>> {
    const result = await apiClient.get(methodsPath(), { params }).then(res => res.data as ServerPagedResult<ShippingMethodListItem>)
    if (result.isSuccess) {
      return mapItems(result, d => ({ ...d, statusLabel: d.isActive ? 'Active' : 'Inactive' }))
    }
    return result as ServerPagedResult<ShippingMethodListItemModel>
  },

  async getById(id: string): Promise<ServerResult<ShippingMethodDetailModel>> {
    const result = await apiClient.get(methodsPath(id)).then(res => res.data as ServerResult<ShippingMethodDetail>)
    if (result.isSuccess) {
      return mapValue(result, d => ({ ...d, statusLabel: d.isActive ? 'Active' : 'Inactive' }))
    }
    return result as ServerResult<ShippingMethodDetailModel>
  },

  async create(data: CreateShippingMethodRequest): Promise<ServerResult<ShippingMethodDetailModel>> {
    const result = await apiClient.post(methodsPath(), data).then(res => res.data as ServerResult<ShippingMethodDetail>)
    if (result.isSuccess) {
      return mapValue(result, d => ({ ...d, statusLabel: d.isActive ? 'Active' : 'Inactive' }))
    }
    return result as ServerResult<ShippingMethodDetailModel>
  },

  async update(id: string, data: UpdateShippingMethodRequest): Promise<ServerResult<ShippingMethodDetailModel>> {
    const result = await apiClient.put(methodsPath(id), data).then(res => res.data as ServerResult<ShippingMethodDetail>)
    if (result.isSuccess) {
      return mapValue(result, d => ({ ...d, statusLabel: d.isActive ? 'Active' : 'Inactive' }))
    }
    return result as ServerResult<ShippingMethodDetailModel>
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
