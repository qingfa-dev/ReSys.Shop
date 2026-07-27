import { z } from 'zod'
export type TFunction = (key: string) => string
export class UserFields {
  constructor(private t: TFunction) {}
  email() { return z.string().min(1, 'Email is required').email('Invalid email') }
  userName() { return z.string().min(1, 'Username is required') }
  password() { return z.string().min(1, 'Password is required').min(6, 'Password must be at least 6 characters') }
  firstName() { return z.string().min(1, 'First name is required') }
  lastName() { return z.string().min(1, 'Last name is required') }
  phone() { return z.string().optional() }
  isActive() { return z.boolean().optional() }
}
