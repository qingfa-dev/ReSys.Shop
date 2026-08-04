// Types mirror the storefront payment DTOs exactly (camelCase JSON).
// Contracts pinned from Module.Payment.Features.Storefront.Payment:
// - Methods:       StorePaymentMethodListItemResponse (PagedResult)
// - CreateIntent:  CreatePaymentIntent.Request / StorePaymentDetailResponse
// - Confirm:       ConfirmPayment.Response (StorePaymentDetailResponse + message)
// - SetupIntent:   CreateSetupIntent.Request / StorePaymentDetailResponse
// Enums (DisplayOn, PaymentRecordState) serialize as strings via JsonStringEnumConverter.

export type PaymentDisplayOn = 'Both' | 'Frontend' | 'Backend'

// StorePaymentMethodListItemResponse — GET api/storefront/payment/methods (paged).
export interface PaymentMethod {
  id: string
  name: string
  code: string | null
  description: string | null
  providerKey: string
  preferences: Record<string, string> | null
  active: boolean
  autoCapture: boolean
  displayOn: PaymentDisplayOn
  position: number
  presentation: string | null
  webhookEnabled: boolean
}

// StorePaymentDetailResponse — shared detail shape for create-intent / confirm / setup-intent.
export interface PaymentIntent {
  id: string
  amount: number
  currency: string
  orderId: string
  paymentMethodId: string
  state: string
  paymentStatus: string | null
  clientSecret: string | null
  responseCode: string | null
  createdAtUtc: string
  modifiedAtUtc: string | null
}

// CreatePaymentIntent.Request — POST api/storefront/payment/create-intent.
// The backend derives amount/currency from the order; only orderId is required.
export interface CreatePaymentIntentRequest {
  orderId: string
  paymentMethodId?: string
  returnUrl?: string
}

// CreateSetupIntent.Request — POST api/storefront/payment/setup-intent.
// The handler only consumes paymentMethodId; the rest of PaymentParameters is unused.
export interface CreateSetupIntentRequest {
  paymentMethodId: string
}

// ConfirmPayment.Response — POST api/storefront/payment/confirm/{paymentId}.
export interface ConfirmPaymentResponse extends PaymentIntent {
  message: string
}
