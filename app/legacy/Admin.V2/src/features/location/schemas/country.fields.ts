import { z } from 'zod'
export type TFunction = (key: string) => string
export class CountryFields {
  constructor(private t: TFunction) {}
  name() { return z.string().min(1, 'Name is required') }
  isoCode() { return z.string().length(2, 'ISO code must be exactly 2 characters') }
  iso3Code() { return z.string().length(3).optional() }
  numericCode() { return z.string().optional() }
  phoneCode() { return z.string().optional() }
  isActive() { return z.boolean().optional() }
}
