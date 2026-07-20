import apiClient from '@/common/api/http/api.client'
import { PAYMENTS } from '@/common/api/constants'
import type { ServerPagedResult, ServerResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { PaymentListItem, PaymentDetail } from '../types/payment.response.type'
import type { PaymentListItemModel, PaymentDetailModel } from '../types/payment.model.type'
import type { CapturePaymentRequest, RefundPaymentRequest } from '../types/payment.request.type'
import { mapPaymentListItem, mapPaymentDetail } from '../mappers/payment.mapper'

function paymentsPath(sub?: string): string {
  return `${PAYMENTS}/payments${sub ? `/${sub}` : ''}`
}

export const paymentRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<PaymentListItemModel>> {
    const result = await apiClient.get(paymentsPath(), { params }).then(res => res.data as ServerPagedResult<PaymentListItem>)
    if (result.isSuccess) {
      return { ...result, items: result.items.map(mapPaymentListItem) }
    }
    return result as ServerPagedResult<PaymentListItemModel>
  },

  async getById(id: string): Promise<ServerResult<PaymentDetailModel>> {
    const result = await apiClient.get(paymentsPath(id)).then(res => res.data as ServerResult<PaymentDetail>)
    if (result.isSuccess) {
      return { ...result, value: mapPaymentDetail(result.value) }
    }
    return result as ServerResult<PaymentDetailModel>
  },

  capture(id: string, amount?: number): Promise<ServerResult<void>> {
    return apiClient.post(paymentsPath(`${id}/capture`), amount !== undefined ? { amount } : undefined).then(res => res.data as ServerResult<void>)
  },

  void(id: string): Promise<ServerResult<void>> {
    return apiClient.post(paymentsPath(`${id}/void`)).then(res => res.data as ServerResult<void>)
  },

  refund(id: string, data?: RefundPaymentRequest): Promise<ServerResult<void>> {
    return apiClient.post(paymentsPath(`${id}/refund`), data).then(res => res.data as ServerResult<void>)
  },
}
