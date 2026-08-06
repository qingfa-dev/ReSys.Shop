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

// GET api/storefront/payment/methods returns a paged result of active methods.
export function getPaymentMethods(params: QueryingParameters = {}): Promise<PagedResult<PaymentMethod>> {
  return getPaged<PaymentMethod>(ENDPOINTS.paymentMethods, params)
}

// POST api/storefront/payment/confirm/{paymentId} — no request body.
export function confirmPayment(paymentId: string): Promise<Result<ConfirmPaymentResponse>> {
  return post<Result<ConfirmPaymentResponse>>(ENDPOINTS.paymentConfirm(paymentId))
}

// POST api/storefront/payment/setup-intent — creates a Stripe SetupIntent for saving a payment method.
export function createSetupIntent(req: CreateSetupIntentRequest): Promise<Result<PaymentIntent>> {
  return post<Result<PaymentIntent>>(ENDPOINTS.paymentSetupIntent, req)
}
