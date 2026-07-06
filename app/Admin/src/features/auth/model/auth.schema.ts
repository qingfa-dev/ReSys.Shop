import { z } from 'zod'
import type { LoginRequest } from './auth.types'

export const loginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
}) satisfies z.ZodType<LoginRequest>
