import type { PaymentMethodParameters } from './PaymentMethod.Parameters.Type'
export type CreatePaymentMethodRequest = PaymentMethodParameters
export type UpdatePaymentMethodRequest = Partial<CreatePaymentMethodRequest>
