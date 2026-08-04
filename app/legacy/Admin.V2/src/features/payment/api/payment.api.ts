import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { PaymentResponse, CapturePaymentRequest, VoidPaymentRequest, RefundPaymentRequest } from '../types'

export class PaymentApi {
  static getMany(query: ListQuery): Promise<PagedResult<PaymentResponse>> {
    return getPagedList<PaymentResponse>('/payment/payments', query)
  }
  static async get(id: string): Promise<Result<PaymentResponse>> {
    const res = await apiClient.get<Result<PaymentResponse>>(`/payment/payments/${id}`)
    return res.data
  }
  static async capture(id: string, data?: CapturePaymentRequest): Promise<Result<PaymentResponse>> {
    const res = await apiClient.post<Result<PaymentResponse>>(`/payment/payments/${id}/capture`, data)
    return res.data
  }
  static async void(id: string, data?: VoidPaymentRequest): Promise<Result<PaymentResponse>> {
    const res = await apiClient.post<Result<PaymentResponse>>(`/payment/payments/${id}/void`, data)
    return res.data
  }
  static async refund(id: string, data?: RefundPaymentRequest): Promise<Result<PaymentResponse>> {
    const res = await apiClient.post<Result<PaymentResponse>>(`/payment/payments/${id}/refund`, data)
    return res.data
  }
}
