import { z } from 'zod'

export const UserSchema = z.object({
  email: z.string().email('Invalid email format').min(1, 'Email is required').max(255, 'Email must not exceed 255 characters'),
  firstName: z.string().min(1, 'First name is required').max(100, 'First name must not exceed 100 characters'),
  lastName: z.string().min(1, 'Last name is required').max(100, 'Last name must not exceed 100 characters'),
  role: z.array(z.string()).min(1, 'At least one role must be assigned'),
  password: z.string().min(6, 'Password must be at least 6 characters').max(128, 'Password must not exceed 128 characters').optional(),
  phoneNumber: z.string().max(30, 'Phone number must not exceed 30 characters').optional(),
  emailConfirmed: z.boolean().optional(),
  isActive: z.boolean().optional().default(true),
})

export type UserParameters = z.infer<typeof UserSchema>
