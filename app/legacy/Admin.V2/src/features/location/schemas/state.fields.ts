import { z } from 'zod'
export class StateFields {
  constructor(private t: (key: string) => string) {}
  name() { return z.string().min(1, 'Name is required') }
  isoCode() { return z.string().min(1, 'ISO code is required') }
  countryId() { return z.string().min(1, 'Country is required') }
  isActive() { return z.boolean().optional() }
}
