import { z } from 'zod'
export type TFunction = (key: string) => string
export class OrderFields {
  constructor(private t: TFunction) {}
  customerId() { return z.string().min(1, 'Customer is required') }
  notes() { return z.string().optional() }
  lineItems() { return z.array(z.object({ variantId: z.string().min(1), quantity: z.number().min(1), unitPrice: z.number().min(0) })).min(1, 'At least one line item is required') }
  firstName() { return z.string().min(1, 'First name is required') }
  lastName() { return z.string().min(1, 'Last name is required') }
  address1() { return z.string().min(1, 'Address is required') }
  address2() { return z.string().optional() }
  city() { return z.string().min(1, 'City is required') }
  state() { return z.string().optional() }
  postalCode() { return z.string().min(1, 'Postal code is required') }
  country() { return z.string().min(1, 'Country is required') }
  phone() { return z.string().optional() }
}
