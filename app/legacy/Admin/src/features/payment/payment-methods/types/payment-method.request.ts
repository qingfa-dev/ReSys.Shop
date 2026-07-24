import type { PaymentMethodParameters } from './payment-method.parameters'
export type CreatePaymentMethodRequest = PaymentMethodParameters
export type UpdatePaymentMethodRequest = Partial<CreatePaymentMethodRequest>
