import { paymentRepository } from '../api/payment.api'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { PaymentListItemModel, PaymentDetailModel } from '../types/payment.model.type'
import { mapPaymentListItem, mapPaymentDetail } from '../mappers/payment.mapper'

export const paymentService = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<PaymentListItemModel>> {
    const result = await paymentRepository.list(params)
    if (result.isSuccess) {
      return { ...result, items: result.items.map(mapPaymentListItem) }
    }
    return result as ServerPagedResult<PaymentListItemModel>
  },
  async getById(id: string): Promise<ServerResult<PaymentDetailModel>> {
    const result = await paymentRepository.getById(id)
    if (result.isSuccess) {
      return { ...result, value: mapPaymentDetail(result.value) }
    }
    return result as ServerResult<PaymentDetailModel>
  },
  capture: paymentRepository.capture,
  void: paymentRepository.void,
  refund(id: string, amount?: number, reason?: string) {
    return paymentRepository.refund(id, amount !== undefined ? { amount, reason } : undefined)
  },
}
