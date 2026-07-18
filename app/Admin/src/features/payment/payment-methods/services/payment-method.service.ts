import { paymentMethodRepository } from '../api/payment-method.api'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { PaymentMethodListItemModel, PaymentMethodDetailModel } from '../types/payment-method.model.type'
import { mapPaymentMethodListItem, mapPaymentMethodDetail } from '../mappers/payment-method.mapper'

export const paymentMethodService = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<PaymentMethodListItemModel>> {
    const result = await paymentMethodRepository.list(params)
    if (result.isSuccess) {
      return { ...result, items: result.items.map(mapPaymentMethodListItem) }
    }
    return result as ServerPagedResult<PaymentMethodListItemModel>
  },
  async getById(id: string): Promise<ServerResult<PaymentMethodDetailModel>> {
    const result = await paymentMethodRepository.getById(id)
    if (result.isSuccess) {
      return { ...result, value: mapPaymentMethodDetail(result.value) }
    }
    return result as ServerResult<PaymentMethodDetailModel>
  },
  async create(data: Record<string, unknown>): Promise<ServerResult<PaymentMethodDetailModel>> {
    const result = await paymentMethodRepository.create(data as never)
    if (result.isSuccess) {
      return { ...result, value: mapPaymentMethodDetail(result.value) }
    }
    return result as ServerResult<PaymentMethodDetailModel>
  },
  async update(id: string, data: Record<string, unknown>): Promise<ServerResult<PaymentMethodDetailModel>> {
    const result = await paymentMethodRepository.update(id, data as never)
    if (result.isSuccess) {
      return { ...result, value: mapPaymentMethodDetail(result.value) }
    }
    return result as ServerResult<PaymentMethodDetailModel>
  },
  delete: paymentMethodRepository.delete,
  activate: paymentMethodRepository.activate,
  deactivate: paymentMethodRepository.deactivate,
}
