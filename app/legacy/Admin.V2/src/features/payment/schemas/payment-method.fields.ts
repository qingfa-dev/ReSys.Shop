import { z } from 'zod'
export type TFunction = (key: string) => string
export class PaymentMethodFields {
  constructor(private t: TFunction) {}
  name() { return z.string().min(1, 'Name is required') }
  code() { return z.string().min(1, 'Code is required') }
  description() { return z.string().optional() }
  isActive() { return z.boolean().optional() }
  isTestMode() { return z.boolean().optional() }
  displayOrder() { return z.coerce.number().int().min(0).optional() }
  supportedCurrencies() { return z.string().optional() }
}
