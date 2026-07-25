import { z } from 'zod'
export type TFunction = (key: string) => string
export class ProfileFields {
  constructor(private t: TFunction) {}
  firstName() { return z.string().min(1, 'First name is required') }
  lastName() { return z.string().min(1, 'Last name is required') }
  phone() { return z.string().optional() }
  avatarUrl() { return z.string().url().optional().or(z.literal('')) }
  dateOfBirth() { return z.string().optional() }
}
