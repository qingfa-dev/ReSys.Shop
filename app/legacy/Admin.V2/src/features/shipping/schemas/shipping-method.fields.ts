import { z } from 'zod'
export type TFunction = (key: string) => string
export class ShippingMethodFields {
  constructor(private t: TFunction) {}
  name() { return z.string().min(1, 'Name is required') }
  code() { return z.string().min(1, 'Code is required') }
  description() { return z.string().optional() }
  isActive() { return z.boolean().optional() }
  displayOrder() { return z.coerce.number().int().min(0).optional() }
  estimatedDeliveryMin() { return z.coerce.number().int().min(0).optional().nullable() }
  estimatedDeliveryMax() { return z.coerce.number().int().min(0).optional().nullable() }
}
