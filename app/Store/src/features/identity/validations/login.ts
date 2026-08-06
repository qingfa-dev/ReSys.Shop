import { z } from 'zod'

export const loginSchema = z.object({
  credential: z.string().min(1, 'Email is required').email('Enter a valid email address'),
  password: z.string().min(1, 'Password is required'),
})
