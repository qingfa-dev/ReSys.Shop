import apiClient from '@/shared/api/http/api.client'
import { PAYMENTS } from '@/shared/api/constants'
import type { ServerPagedResult, ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { PaymentMethodListItem, PaymentMethodDetail } from '../types/PaymentMethod.Response.Type'
import type { CreatePaymentMethodRequest, UpdatePaymentMethodRequest } from '../types/PaymentMethod.Request.Type'

function methodsPath(sub?: string): string {
  return `${PAYMENTS}/payment-methods${sub ? `/${sub}` : ''}`
}

export const paymentMethodRepository = {
  list(params?: ServerQueryingParameters): Promise<ServerPagedResult<PaymentMethodListItem>> {
    return apiClient.get(methodsPath(), { params }).then(res => res.data as ServerPagedResult<PaymentMethodListItem>)
  },

  getById(id: string): Promise<ServerResult<PaymentMethodDetail>> {
    return apiClient.get(methodsPath(id)).then(res => res.data as ServerResult<PaymentMethodDetail>)
  },

  create(data: CreatePaymentMethodRequest): Promise<ServerResult<PaymentMethodDetail>> {
    return apiClient.post(methodsPath(), data).then(res => res.data as ServerResult<PaymentMethodDetail>)
  },

  update(id: string, data: UpdatePaymentMethodRequest): Promise<ServerResult<PaymentMethodDetail>> {
    return apiClient.put(methodsPath(id), data).then(res => res.data as ServerResult<PaymentMethodDetail>)
  },

  delete(id: string): Promise<ServerResult<void>> {
    return apiClient.delete(methodsPath(id)).then(res => res.data as ServerResult<void>)
  },

  activate(id: string): Promise<ServerResult<void>> {
    return apiClient.patch(methodsPath(`${id}/activate`)).then(res => res.data as ServerResult<void>)
  },

  deactivate(id: string): Promise<ServerResult<void>> {
    return apiClient.patch(methodsPath(`${id}/deactivate`)).then(res => res.data as ServerResult<void>)
  },
}
