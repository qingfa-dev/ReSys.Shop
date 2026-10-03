import { getPaged } from '@/shared/api'
import { get, post, put, del, patch } from '@/shared/api/client'
import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  PaymentMethodDetail,
  PaymentMethodListItem,
  PaymentMethodRequest,
  PaymentMethodUpdateRequest,
} from '../types/paymentMethod'
import {
  PAYMENT_METHOD_FILTER_FIELDS,
  PAYMENT_METHOD_SORT_FIELDS,
  PAYMENT_METHOD_SEARCH_FIELDS,
} from '../types/paymentMethod'

export class PaymentMethodApi {
  static getPaymentMethods(params: QueryingParameters): Promise<PagedResult<PaymentMethodListItem>> {
    return getPaged<PaymentMethodListItem>('/api/admin/billing/payment-methods', params, {
      allowedFilterFields: PAYMENT_METHOD_FILTER_FIELDS,
      allowedSortFields: PAYMENT_METHOD_SORT_FIELDS,
      allowedSearchFields: PAYMENT_METHOD_SEARCH_FIELDS,
    })
  }

  static getPaymentMethod(id: string): Promise<Result<PaymentMethodDetail>> {
    return get<Result<PaymentMethodDetail>>(`/api/admin/billing/payment-methods/${id}`)
  }

  static createPaymentMethod(request: PaymentMethodRequest): Promise<Result<PaymentMethodDetail>> {
    return post<Result<PaymentMethodDetail>>('/api/admin/billing/payment-methods', request)
  }

  static updatePaymentMethod(id: string, request: PaymentMethodUpdateRequest): Promise<Result<PaymentMethodDetail>> {
    return put<Result<PaymentMethodDetail>>(`/api/admin/billing/payment-methods/${id}`, request)
  }

  static deletePaymentMethod(id: string): Promise<Result<void>> {
    return del<Result<void>>(`/api/admin/billing/payment-methods/${id}`)
  }

  static activatePaymentMethod(id: string): Promise<Result<void>> {
    return patch<Result<void>>(`/api/admin/billing/payment-methods/${id}/activate`)
  }

  static deactivatePaymentMethod(id: string): Promise<Result<void>> {
    return patch<Result<void>>(`/api/admin/billing/payment-methods/${id}/deactivate`)
  }
}