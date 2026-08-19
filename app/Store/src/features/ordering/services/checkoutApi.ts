import { post, patch, get } from '@/shared/api/client'
import { PaymentIntentResponseSchema, PlaceOrderResponseSchema } from '../validations/checkout'
import type { Result } from '@/shared/types'
import type {
  UpdateCheckoutRequest,
  SelectShippingRateRequest,
  CreatePaymentIntentRequest,
  PaymentIntentResponse,
  PlaceOrderRequest,
  PlaceOrderResponse,
} from '../types'

// Service: Checkout API client — handles address, shipping, payment, and order placement.
export class CheckoutApi {
  static async updateCheckout(req: UpdateCheckoutRequest): Promise<Result<void>> {
    // Update: Persist address and email to the active cart checkout session.
    return await patch<Result<void>>('/api/storefront/cart', req)
  }

  static async selectShippingRate(req: SelectShippingRateRequest): Promise<Result<void>> {
    return await patch<Result<void>>('/api/storefront/cart/shipping-rate', req)
  }

  static async validateCheckout(): Promise<Result<void>> {
    return await get<Result<void>>('/api/storefront/cart/checkout')
  }

  // Call: Create payment intent with gateway — returns client secret for frontend confirmation.
  static async createPaymentIntent(req: CreatePaymentIntentRequest): Promise<Result<PaymentIntentResponse>> {
    const result = await post<Result<PaymentIntentResponse>>('/api/storefront/cart/payment/intent', req)
    if (!result.isSuccess) return result
    // Validate: Parse payment intent response for type-safe client secret handling.
    result.value = PaymentIntentResponseSchema.parse(result.value)
    return result
  }

  // Call: Finalize checkout — converts cart to order and processes payment.
  static async placeOrder(req: PlaceOrderRequest): Promise<Result<PlaceOrderResponse>> {
    const result = await post<Result<PlaceOrderResponse>>('/api/storefront/cart/checkout', req)
    if (!result.isSuccess) return result
    result.value = PlaceOrderResponseSchema.parse(result.value)
    return result
  }
}
