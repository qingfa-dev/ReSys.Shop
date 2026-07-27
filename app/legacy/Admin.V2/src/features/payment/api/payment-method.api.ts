import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { PaymentMethodResponse, CreatePaymentMethodRequest, UpdatePaymentMethodRequest } from '../types'

export class PaymentMethodApi {
  static getMany(query: ListQuery): Promise<PagedResult<PaymentMethodResponse>> {
    return getPagedList<PaymentMethodResponse>('/payment/payment-methods', query)
  }
  static async get(id: string): Promise<Result<PaymentMethodResponse>> {
    const res = await apiClient.get<Result<PaymentMethodResponse>>(`/payment/payment-methods/${id}`)
    return res.data
  }
  static async create(data: CreatePaymentMethodRequest): Promise<Result<PaymentMethodResponse>> {
    const res = await apiClient.post<Result<PaymentMethodResponse>>('/payment/payment-methods', data)
    return res.data
  }
  static async update(id: string, data: UpdatePaymentMethodRequest): Promise<Result<PaymentMethodResponse>> {
    const res = await apiClient.put<Result<PaymentMethodResponse>>(`/payment/payment-methods/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/payment/payment-methods/${id}`)
    return res.data
  }
  static async activate(id: string): Promise<Result<void>> {
    const res = await apiClient.patch<Result<void>>(`/payment/payment-methods/${id}/activate`)
    return res.data
  }
  static async deactivate(id: string): Promise<Result<void>> {
    const res = await apiClient.patch<Result<void>>(`/payment/payment-methods/${id}/deactivate`)
    return res.data
  }
}
