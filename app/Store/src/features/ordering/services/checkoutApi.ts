import { post, put } from '@/shared/api/client'
import { CART, PAYMENT } from '@/shared/constants/api'
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

export class CheckoutApi {
  static async updateCheckout(req: UpdateCheckoutRequest): Promise<Result<void>> {
    return await put<Result<void>>(`${CART}`, req)
  }

  static async selectShippingRate(req: SelectShippingRateRequest): Promise<Result<void>> {
    return await post<Result<void>>(`${CART}/shipping-rate`, req)
  }

  static async validateCheckout(): Promise<Result<void>> {
    return await post<Result<void>>(`${CART}/validate`)
  }

  static async createPaymentIntent(req: CreatePaymentIntentRequest): Promise<Result<PaymentIntentResponse>> {
    const result = await post<Result<PaymentIntentResponse>>(`${PAYMENT}/create-intent`, req)
    if (!result.isSuccess) return result
    result.value = PaymentIntentResponseSchema.parse(result.value)
    return result
  }

  static async placeOrder(req: PlaceOrderRequest): Promise<Result<PlaceOrderResponse>> {
    const result = await post<Result<PlaceOrderResponse>>(`${CART}/checkout`, req)
    if (!result.isSuccess) return result
    result.value = PlaceOrderResponseSchema.parse(result.value)
    return result
  }
}
