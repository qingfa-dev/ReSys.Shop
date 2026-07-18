import apiClient from '@/shared/api/http/api.client'
import { PAYMENTS } from '@/shared/api/constants'
import type { ServerPagedResult, ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { PaymentListItem, PaymentDetail } from '../types/Payment.Response.Type'
import type { CapturePaymentRequest, RefundPaymentRequest } from '../types/Payment.Request.Type'

function paymentsPath(sub?: string): string {
  return `${PAYMENTS}/payments${sub ? `/${sub}` : ''}`
}

export const paymentRepository = {
  list(params?: ServerQueryingParameters): Promise<ServerPagedResult<PaymentListItem>> {
    return apiClient.get(paymentsPath(), { params }).then(res => res.data as ServerPagedResult<PaymentListItem>)
  },

  getById(id: string): Promise<ServerResult<PaymentDetail>> {
    return apiClient.get(paymentsPath(id)).then(res => res.data as ServerResult<PaymentDetail>)
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
