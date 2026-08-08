import { getPaged, post } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult, Result } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type {
  ConfirmPaymentResponse,
  CreateSetupIntentRequest,
  PaymentIntent,
  PaymentMethod,
} from '../types/payment'

// Call: Storefront payment API - fetch active payment methods (paged)
export function getPaymentMethods(params: QueryingParameters = {}): Promise<PagedResult<PaymentMethod>> {
  return getPaged<PaymentMethod>(ENDPOINTS.paymentMethods, params)
}

// Call: Storefront payment API - confirm payment by ID (no request body)
export function confirmPayment(paymentId: string): Promise<Result<ConfirmPaymentResponse>> {
  return post<Result<ConfirmPaymentResponse>>(ENDPOINTS.paymentConfirm(paymentId))
}

// Call: Storefront payment API - create Stripe SetupIntent for saving payment method
export function createSetupIntent(req: CreateSetupIntentRequest): Promise<Result<PaymentIntent>> {
  return post<Result<PaymentIntent>>(ENDPOINTS.paymentSetupIntent, req)
}
