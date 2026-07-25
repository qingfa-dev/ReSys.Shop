import { z } from 'zod'
export type TFunction = (key: string) => string
export class ShippingRateFields {
  constructor(private t: TFunction) {}
  shippingMethodId() { return z.string().min(1, 'Shipping method is required') }
  name() { return z.string().min(1, 'Name is required') }
  rate() { return z.coerce.number().min(0, 'Rate must be >= 0') }
  currency() { return z.string().min(1, 'Currency is required') }
  minOrderAmount() { return z.coerce.number().min(0).optional().nullable() }
  maxOrderAmount() { return z.coerce.number().min(0).optional().nullable() }
  minWeight() { return z.coerce.number().min(0).optional().nullable() }
  maxWeight() { return z.coerce.number().min(0).optional().nullable() }
}
