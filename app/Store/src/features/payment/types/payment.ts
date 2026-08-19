// Types mirror the storefront payment DTOs exactly (camelCase JSON).
// Contracts pinned from Module.Payment.Features.Storefront.Payment:
// - Methods:       StorePaymentMethodListItemResponse (PagedResult)
// - CreateIntent:  CreatePaymentIntent.Request / StorePaymentDetailResponse
// - Confirm:       ConfirmPayment.Response (StorePaymentDetailResponse + message)
// - SetupIntent:   CreateSetupIntent.Request / StorePaymentDetailResponse
// Enums (DisplayOn) serialize as strings via JsonStringEnumConverter.

export type PaymentDisplayOn = 'Both' | 'Frontend' | 'Backend'

// PaymentRecordState serializes as a JSON string via JsonStringEnumConverter.
export type PaymentRecordState =
  | 'Checkout'
  | 'Processing'
  | 'Pending'
  | 'Completed'
  | 'Failed'
  | 'Void'
  | 'Disputed'
  | 'Invalid'

// Severity: PrimeVue Tag severity per payment record state (mirrors Admin SPA).
export const PAYMENT_STATE_SEVERITY: Record<PaymentRecordState, string> = {
  Checkout: 'warn',
  Processing: 'warn',
  Pending: 'warn',
  Completed: 'success',
  Failed: 'danger',
  Void: 'secondary',
  Disputed: 'secondary',
  Invalid: 'danger',
}

// Label: Wire-equivalent display label per payment record state.
export const PAYMENT_STATE_LABEL: Record<PaymentRecordState, string> = {
  Checkout: 'Checkout',
  Processing: 'Processing',
  Pending: 'Pending',
  Completed: 'Completed',
  Failed: 'Failed',
  Void: 'Void',
  Disputed: 'Disputed',
  Invalid: 'Invalid',
}

// StorePaymentMethodListItemResponse — GET api/storefront/payment/methods (paged).
export interface PaymentMethod {
  id: string
  name: string
  code: string | null
  description: string | null
  providerKey: string
  // Settings inherited from PaymentMethodParameters but not set by the
  // storefront mapping (stays null) — PaymentMethod.Model.cs:24.
  settings?: Record<string, string> | null
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
  state: PaymentRecordState
  paymentStatus: string | null
  clientSecret: string | null
  // CheckoutUrl — StorePaymentDetailResponse (Storefront.Payment.Model.cs:17).
  checkoutUrl?: string | null
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

// PaymentStatusResponse: GET api/storefront/cart/payment/intent/{orderId} (poll).
export interface PaymentStatusResponse {
  id: string
  orderId: string
  amount: number
  currency: string
  state: PaymentRecordState
  isCompleted: boolean
}
