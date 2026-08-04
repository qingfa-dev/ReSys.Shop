import apiClient from '@/common/api/http/api.client'
import { PAYMENTS } from '@/common/api/constants'
import type { ServerPagedResult, ServerResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { PaymentMethodListItem, PaymentMethodDetail } from '../types/payment-method.response'
import type { PaymentMethodListItemModel, PaymentMethodDetailModel } from '../types/payment-method.model'
import type { CreatePaymentMethodRequest, UpdatePaymentMethodRequest } from '../types/payment-method.request'
import { mapValue, mapItems } from '@/common/utils/transform'

function methodsPath(sub?: string): string {
  return `${PAYMENTS}/payment-methods${sub ? `/${sub}` : ''}`
}

export const paymentMethodRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<PaymentMethodListItemModel>> {
    const result = await apiClient.get(methodsPath(), { params }).then(res => res.data as ServerPagedResult<PaymentMethodListItem>)
    if (result.isSuccess) {
      return mapItems(result, d => ({ ...d, statusLabel: d.isActive ? 'Active' : 'Inactive' }))
    }
    return result as ServerPagedResult<PaymentMethodListItemModel>
  },

  async getById(id: string): Promise<ServerResult<PaymentMethodDetailModel>> {
    const result = await apiClient.get(methodsPath(id)).then(res => res.data as ServerResult<PaymentMethodDetail>)
    if (result.isSuccess) {
      return mapValue(result, d => ({ ...d, statusLabel: d.isActive ? 'Active' : 'Inactive' }))
    }
    return result as ServerResult<PaymentMethodDetailModel>
  },

  async create(data: CreatePaymentMethodRequest): Promise<ServerResult<PaymentMethodDetailModel>> {
    const result = await apiClient.post(methodsPath(), data).then(res => res.data as ServerResult<PaymentMethodDetail>)
    if (result.isSuccess) {
      return mapValue(result, d => ({ ...d, statusLabel: d.isActive ? 'Active' : 'Inactive' }))
    }
    return result as ServerResult<PaymentMethodDetailModel>
  },

  async update(id: string, data: UpdatePaymentMethodRequest): Promise<ServerResult<PaymentMethodDetailModel>> {
    const result = await apiClient.put(methodsPath(id), data).then(res => res.data as ServerResult<PaymentMethodDetail>)
    if (result.isSuccess) {
      return mapValue(result, d => ({ ...d, statusLabel: d.isActive ? 'Active' : 'Inactive' }))
    }
    return result as ServerResult<PaymentMethodDetailModel>
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
