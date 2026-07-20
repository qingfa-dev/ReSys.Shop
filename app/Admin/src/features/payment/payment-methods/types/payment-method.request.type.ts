import type { PaymentMethodParameters } from './payment-method.parameters.type'
export type CreatePaymentMethodRequest = PaymentMethodParameters
export type UpdatePaymentMethodRequest = Partial<CreatePaymentMethodRequest>
