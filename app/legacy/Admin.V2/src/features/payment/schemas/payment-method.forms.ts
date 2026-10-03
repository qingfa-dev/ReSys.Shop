import { z } from 'zod'
import type { TFunction } from './payment-method.fields'
import { PaymentMethodFields } from './payment-method.fields'

export class PaymentMethodForms {
  private f: PaymentMethodFields
  constructor(private t: TFunction) { this.f = new PaymentMethodFields(t) }
  create() { return z.object({ name: this.f.name(), code: this.f.code(), description: this.f.description(), isActive: this.f.isActive(), isTestMode: this.f.isTestMode(), displayOrder: this.f.displayOrder(), supportedCurrencies: this.f.supportedCurrencies() }) }
  update() { return this.create() }
}
export type CreatePaymentMethodForm = z.input<ReturnType<PaymentMethodForms['create']>>
export type UpdatePaymentMethodForm = z.input<ReturnType<PaymentMethodForms['update']>>
