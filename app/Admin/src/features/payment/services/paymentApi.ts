import { getPaged } from '@/shared/api'
import { get, post } from '@/shared/api/client'
import type { Result, PagedResult } from '@/shared/types'
import type {
  PaymentListItem,
  PaymentDetail,
  PaymentQuery,
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
  toPaymentQueryParams,
} from '../types/payment'

export class PaymentApi {
  private static readonly BASE = 'api/admin/billing/payments'

  static getPayments(query: PaymentQuery): Promise<PagedResult<PaymentListItem>> {
    return getPaged<PaymentListItem>(PaymentApi.BASE, toPaymentQueryParams(query), {
      allowedFilterFields: PAYMENT_FILTER_FIELDS,
      allowedSortFields: PAYMENT_SORT_FIELDS,
      allowedSearchFields: PAYMENT_SEARCH_FIELDS,
    })
  }

  static getPayment(id: string): Promise<Result<PaymentDetail>> {
    return get<Result<PaymentDetail>>(`${PaymentApi.BASE}/${id}`)
  }

  static capturePayment(id: string, request?: CapturePaymentRequest): Promise<Result<CapturePaymentResponse>> {
    return post<Result<CapturePaymentResponse>>(`${PaymentApi.BASE}/${id}/capture`, request ?? {})
  }

  static refundPayment(id: string, request: RefundPaymentRequest): Promise<Result<RefundPaymentResponse>> {
    return post<Result<RefundPaymentResponse>>(`${PaymentApi.BASE}/${id}/refund`, request)
  }

  static voidPayment(id: string): Promise<Result<VoidPaymentResponse>> {
    return post<Result<VoidPaymentResponse>>(`${PaymentApi.BASE}/${id}/void`)
  }
}
