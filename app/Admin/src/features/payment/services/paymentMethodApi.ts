import { getPaged } from '@/shared/api'
import { get, post, put, del, patch } from '@/shared/api/client'
import { PAYMENT } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  PaymentMethodDetail,
  PaymentMethodListItem,
  PaymentMethodQuery,
  PaymentMethodRequest,
  PaymentMethodUpdateRequest,
} from '../types/paymentMethod'
import {
  PAYMENT_METHOD_FILTER_FIELDS,
  PAYMENT_METHOD_SORT_FIELDS,
  PAYMENT_METHOD_SEARCH_FIELDS,
  toPaymentMethodQueryParams,
} from '../types/paymentMethod'

export class PaymentMethodApi {
  private static readonly BASE = `${PAYMENT}/payment-methods`

  static getPaymentMethods(query: PaymentMethodQuery): Promise<PagedResult<PaymentMethodListItem>> {
    return getPaged<PaymentMethodListItem>(PaymentMethodApi.BASE, toPaymentMethodQueryParams(query), {
      allowedFilterFields: PAYMENT_METHOD_FILTER_FIELDS,
      allowedSortFields: PAYMENT_METHOD_SORT_FIELDS,
      allowedSearchFields: PAYMENT_METHOD_SEARCH_FIELDS,
    })
  }

  static getPaymentMethod(id: string): Promise<Result<PaymentMethodDetail>> {
    return get<Result<PaymentMethodDetail>>(`${PaymentMethodApi.BASE}/${id}`)
  }

  static createPaymentMethod(request: PaymentMethodRequest): Promise<Result<PaymentMethodDetail>> {
    return post<Result<PaymentMethodDetail>>(PaymentMethodApi.BASE, request)
  }

  static updatePaymentMethod(id: string, request: PaymentMethodUpdateRequest): Promise<Result<PaymentMethodDetail>> {
    return put<Result<PaymentMethodDetail>>(`${PaymentMethodApi.BASE}/${id}`, request)
  }

  static deletePaymentMethod(id: string): Promise<Result<void>> {
    return del<Result<void>>(`${PaymentMethodApi.BASE}/${id}`)
  }

  static activatePaymentMethod(id: string): Promise<Result<void>> {
    return patch<Result<void>>(`${PaymentMethodApi.BASE}/${id}/activate`)
  }

  static deactivatePaymentMethod(id: string): Promise<Result<void>> {
    return patch<Result<void>>(`${PaymentMethodApi.BASE}/${id}/deactivate`)
  }
}
