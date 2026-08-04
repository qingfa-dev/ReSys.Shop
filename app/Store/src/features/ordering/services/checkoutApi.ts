import { post, put } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'
import type {
  CreatePaymentIntentRequest,
  PaymentIntentResponse,
  PlaceOrderRequest,
  PlaceOrderResponse,
  SelectShippingRateRequest,
  UpdateCheckoutRequest,
} from '../types/checkout'

export function updateCheckout(req: UpdateCheckoutRequest): Promise<Result<unknown>> {
  return put<Result<unknown>>(ENDPOINTS.cart, req)
}

export function selectShippingRate(req: SelectShippingRateRequest): Promise<Result<unknown>> {
  return post<Result<unknown>>(ENDPOINTS.cartShippingRate, req)
}

export function validateCheckout(): Promise<Result<unknown>> {
  return post<Result<unknown>>(ENDPOINTS.cartValidate)
}

export function createPaymentIntent(req: CreatePaymentIntentRequest): Promise<Result<PaymentIntentResponse>> {
  return post<Result<PaymentIntentResponse>>(ENDPOINTS.paymentCreateIntent, req)
}

export function placeOrder(req: PlaceOrderRequest): Promise<Result<PlaceOrderResponse>> {
  return post<Result<PlaceOrderResponse>>(ENDPOINTS.cartCheckout, req)
}
