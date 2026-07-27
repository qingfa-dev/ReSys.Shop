import { z } from 'zod'
export type TFunction = (key: string) => string
export class StockLocationFields {
  constructor(private t: TFunction) {}
  name() { return z.string().min(1, 'Name is required') }
  code() { return z.string().min(1, 'Code is required') }
  address1() { return z.string().optional() }
  address2() { return z.string().optional() }
  city() { return z.string().optional() }
  state() { return z.string().optional() }
  postalCode() { return z.string().optional() }
  country() { return z.string().optional() }
  phone() { return z.string().optional() }
  isDefault() { return z.boolean().optional() }
  isActive() { return z.boolean().optional() }
}
