import { get, getPaged, post } from '@/shared/api'
import type { PagedResult, Result } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type {
  ConfirmPaymentResponse,
  CreateSetupIntentRequest,
  PaymentIntent,
  PaymentMethod,
  PaymentStatusResponse,
} from '../types/payment'

// Call: Storefront payment API - fetch active payment methods (paged)
export function getPaymentMethods(params: QueryingParameters = {}): Promise<PagedResult<PaymentMethod>> {
  return getPaged<PaymentMethod>('/api/storefront/billing/payment-methods', params)
}

// Call: Storefront payment API - confirm payment by ID (no request body)
export function confirmPayment(paymentId: string): Promise<Result<ConfirmPaymentResponse>> {
  return post<Result<ConfirmPaymentResponse>>(`/api/storefront/cart/payment/intent/${paymentId}/confirm`)
}

// Call: Storefront payment API - create Stripe SetupIntent for saving payment method
export function createSetupIntent(req: CreateSetupIntentRequest): Promise<Result<PaymentIntent>> {
  return post<Result<PaymentIntent>>('/api/storefront/billing/payment-methods/setup-intent', req)
}

// Call: Storefront payment API - poll payment status for an order.
export function getPaymentStatus(orderId: string): Promise<Result<PaymentStatusResponse>> {
  return get<Result<PaymentStatusResponse>>(`/api/storefront/cart/payment/intent/${orderId}`)
}
