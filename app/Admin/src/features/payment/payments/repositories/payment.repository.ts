import apiClient from '@/shared/api/http/api.client'
import { PAYMENTS } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface PaymentListItem {
  id: string
  orderId: string
  amount: number
  currency: string
  status: string
  method: string
  createdAt: string
}

export interface PaymentDetail extends PaymentListItem {
  metadata: Record<string, unknown> | null
}

function paymentsPath(sub?: string): string {
  return `${PAYMENTS}/payments${sub ? `/${sub}` : ''}`
}

export const paymentRepository = {
  list(params?: ServerQueryingParameters): Promise<ServerResult<PaymentListItem[]>> {
    return apiClient.get(paymentsPath(), { params }).then(res => res.data as ServerResult<PaymentListItem[]>)
  },

  getById(id: string): Promise<ServerResult<PaymentDetail>> {
    return apiClient.get(paymentsPath(id)).then(res => res.data as ServerResult<PaymentDetail>)
  },

  capture(id: string): Promise<ServerResult<void>> {
    return apiClient.post(paymentsPath(`${id}/capture`)).then(res => res.data as ServerResult<void>)
  },

  void(id: string): Promise<ServerResult<void>> {
    return apiClient.post(paymentsPath(`${id}/void`)).then(res => res.data as ServerResult<void>)
  },

  refund(id: string): Promise<ServerResult<void>> {
    return apiClient.post(paymentsPath(`${id}/refund`)).then(res => res.data as ServerResult<void>)
  },
}
