import type { CreatePaymentMethodForm, UpdatePaymentMethodForm } from '../schemas'
import type { CreatePaymentMethodRequest, UpdatePaymentMethodRequest } from '../types'

export class PaymentMethodFormMapper {
  static toCreate(form: CreatePaymentMethodForm): CreatePaymentMethodRequest { return form }
  static toUpdate(form: UpdatePaymentMethodForm): UpdatePaymentMethodRequest { return form }
}
