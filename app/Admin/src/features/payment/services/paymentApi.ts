import { getPaged } from '@/shared/api'
import { get, post } from '@/shared/api/client'
import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  PaymentListItem,
  PaymentDetail,
  CapturePaymentRequest,
  CapturePaymentResponse,
  RefundPaymentRequest,
  RefundPaymentResponse,
  VoidPaymentResponse,
} from '../types/payment'
import {
  PAYMENT_FILTER_FIELDS,
  PAYMENT_SORT_FIELDS,
  PAYMENT_SEARCH_FIELDS,
} from '../types/payment'

export class PaymentApi {
  static getPayments(params: QueryingParameters): Promise<PagedResult<PaymentListItem>> {
    return getPaged<PaymentListItem>('/api/admin/billing/payments', params, {
      allowedFilterFields: PAYMENT_FILTER_FIELDS,
      allowedSortFields: PAYMENT_SORT_FIELDS,
      allowedSearchFields: PAYMENT_SEARCH_FIELDS,
    })
  }

  static getPayment(id: string): Promise<Result<PaymentDetail>> {
    return get<Result<PaymentDetail>>(`/api/admin/billing/payments/${id}`)
  }

  static capturePayment(id: string, request?: CapturePaymentRequest): Promise<Result<CapturePaymentResponse>> {
    return post<Result<CapturePaymentResponse>>(`/api/admin/billing/payments/${id}/capture`, request ?? {})
  }

  static refundPayment(id: string, request: RefundPaymentRequest): Promise<Result<RefundPaymentResponse>> {
    return post<Result<RefundPaymentResponse>>(`/api/admin/billing/payments/${id}/refund`, request)
  }

  static voidPayment(id: string): Promise<Result<VoidPaymentResponse>> {
    return post<Result<VoidPaymentResponse>>(`/api/admin/billing/payments/${id}/void`)
  }
}