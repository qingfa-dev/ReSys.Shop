import { z } from 'zod'
export class AddressFields {
  constructor(private t: (key: string) => string) {}
  firstName() { return z.string().min(1, 'First name is required') }
  lastName() { return z.string().min(1, 'Last name is required') }
  address1() { return z.string().min(1, 'Address is required') }
  address2() { return z.string().optional() }
  city() { return z.string().min(1, 'City is required') }
  state() { return z.string().optional() }
  postalCode() { return z.string().min(1, 'Postal code is required') }
  country() { return z.string().min(1, 'Country is required') }
  phone() { return z.string().optional() }
  isDefault() { return z.boolean().optional() }
}
